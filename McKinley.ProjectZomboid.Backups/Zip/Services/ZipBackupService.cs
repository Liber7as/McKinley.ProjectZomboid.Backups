using System;
using System.IO;
using System.IO.Abstractions;
using System.IO.Compression;
using System.Threading.Tasks;
using McKinley.ProjectZomboid.Backups.Abstractions;
using McKinley.ProjectZomboid.Backups.Abstractions.Models;
using McKinley.ProjectZomboid.Backups.Zip.Settings;
using Microsoft.Extensions.Logging;

namespace McKinley.ProjectZomboid.Backups.Zip.Services;

public class ZipBackupService : IBackupService
{
    private readonly IFileSystem _fileSystem;
    private readonly ZipBackupSettings _settings;
    private readonly ILogger<ZipBackupService>? _logger;

    public ZipBackupService(IFileSystem fileSystem, ZipBackupSettings settings, ILogger<ZipBackupService>? logger = null)
    {
        _fileSystem = fileSystem;
        _settings = settings;
        _logger = logger;
    }

    public async Task BackupAsync(Save save)
    {
        _logger?.LogInformation($"Backing up save: '{save.Directory.FullName}'");

        // Check to see if the ZIP file already exists
        var zipFileInfo = _fileSystem.FileInfo.New(_settings.FileLocation);

        _logger?.LogInformation(zipFileInfo.Exists ? $"Found backup zip file: '{zipFileInfo.FullName}'" : $"Backup zip file not found. Will create: '{zipFileInfo.FullName}'");

        // Open the ZIP file as a stream, or create a MemoryStream for a new ZIP file. 
        await using Stream zipFileStream = zipFileInfo.Exists
                                               ? _fileSystem.File.Open(zipFileInfo.FullName, FileMode.Open, FileAccess.ReadWrite)
                                               : new MemoryStream();

        // Determine if we should create or update a ZIP file.
        var zipArchiveMode = zipFileInfo.Exists
                                 ? ZipArchiveMode.Update
                                 : ZipArchiveMode.Create;

        _logger?.LogInformation("Beginning file backup...");

        // Create a zip archive from the stream above, and copy the save to it.
        using (var zipArchive = new ZipArchive(zipFileStream, zipArchiveMode, true))
        {
            await CopyDirectoryToZipArchiveAsync(save.Directory, zipArchive);

            _logger?.LogInformation("Completed file backup.");

            _logger?.LogInformation($"Saving zip file: '{zipFileInfo.FullName}'");
        }

        // If the zip archive doesn't exist, save it to the file system.
        if (!zipFileInfo.Exists)
        {
            // Open a new file
            await using var saveStream = zipFileInfo.Create();

            // Ensure the zip file stream is set to the beginning, so we can copy it
            zipFileStream.Seek(0, SeekOrigin.Begin);

            // Copy the zip file to the file system
            await zipFileStream.CopyToAsync(saveStream);

            // Flush to ensure everything is sent
            await zipFileStream.FlushAsync();
            await saveStream.FlushAsync();
        }

        _logger?.LogInformation($"Zip file saved: '{zipFileInfo.FullName}'");
    }

    private async Task CopyDirectoryToZipArchiveAsync(DirectoryInfo directoryInfo, ZipArchive zipArchive)
    {
        // Create a unique timestamp for the backup
        var uniqueDateString = DateTime.UtcNow.ToString("s")
                                       .Replace(":", string.Empty)
                                       .Replace("-", string.Empty)
                                       .Replace("T", string.Empty);

        // Create a folder name for the ZIP file backup
        var folderName = string.Format(_settings.BackupNameFormat, directoryInfo.Name, uniqueDateString);

        // Enumerate all the files in the save
        foreach (var file in directoryInfo.GetFiles("*", SearchOption.AllDirectories))
        {
            // Create a relative path for the file being imported into the ZIP file
            var relativeFileName = file.FullName.Replace(directoryInfo.FullName, string.Empty)
                                       .TrimStart('\\')
                                       .TrimStart('/');

            // Create an entry inside the backup folder for the file
            var entryPath = Path.Combine(folderName, relativeFileName);
            var entry = zipArchive.CreateEntry(entryPath, _settings.CompressionLevel);

            _logger?.LogDebug($"'{file.FullName}' -> '{entry.FullName}'");

            // Open the zip file entry and the file
            await using var zipEntryStream = entry.Open();
            await using var fileStream = file.OpenRead();

            // Copy the file to the ZIP archive
            await fileStream.CopyToAsync(zipEntryStream);

            // Flush to ensure everything is sent
            await fileStream.FlushAsync();
            await zipEntryStream.FlushAsync();
        }
    }
}