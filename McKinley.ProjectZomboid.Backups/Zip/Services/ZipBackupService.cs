using System;
using System.IO;
using System.IO.Abstractions;
using System.IO.Compression;
using System.Threading.Tasks;
using McKinley.ProjectZomboid.Backups.Abstractions.Models;
using McKinley.ProjectZomboid.Backups.Settings;
using Microsoft.Extensions.Logging;

namespace McKinley.ProjectZomboid.Backups.Zip.Services;

public class ZipBackupService : IZipBackupService
{
    private readonly ILogger<ZipBackupService>? _logger;
    private readonly BackupSettings _settings;

    public ZipBackupService(BackupSettings settings, ILogger<ZipBackupService>? logger = null)
    {
        _settings = settings;
        _logger = logger;
    }

    public async Task BackupAsync(Save save, IFileInfo destination)
    {
        _logger?.LogInformation($"Backing up save: '{save.Directory.FullName}'");

        // Check to see if the ZIP file already exists
        _logger?.LogInformation(destination.Exists
                                    ? $"Found backup zip file: '{destination.FullName}'"
                                    : $"Backup zip file not found. Will create: '{destination.FullName}'");

        // Open the ZIP file as a stream, or create a MemoryStream for a new ZIP file. 
        await using Stream zipFileStream = destination.Exists
                                               ? destination.Open(FileMode.Open, FileAccess.ReadWrite)
                                               : new MemoryStream();

        // Determine if we should create or update a ZIP file.
        var zipArchiveMode = destination.Exists
                                 ? ZipArchiveMode.Update
                                 : ZipArchiveMode.Create;

        _logger?.LogInformation("Beginning file backup...");

        // Create a zip archive from the stream above, and copy the save to it.
        using (var zipArchive = new ZipArchive(zipFileStream, zipArchiveMode, true))
        {
            await CopyDirectoryToZipArchiveAsync(save.Directory, zipArchive);

            _logger?.LogInformation("Completed file backup.");

            _logger?.LogInformation($"Saving zip file: '{destination.FullName}'");
        }

        // If the zip archive doesn't exist, save it to the file system.
        if (!destination.Exists)
        {
            // Open a new file
            await using var saveStream = destination.Create();

            // Ensure the zip file stream is set to the beginning, so we can copy it
            zipFileStream.Seek(0, SeekOrigin.Begin);

            // Copy the zip file to the file system
            await zipFileStream.CopyToAsync(saveStream);

            // Flush to ensure everything is sent
            await zipFileStream.FlushAsync();
            await saveStream.FlushAsync();
        }

        _logger?.LogInformation($"Zip file saved: '{destination.FullName}'");
    }

    private async Task CopyDirectoryToZipArchiveAsync(IDirectoryInfo directoryInfo, ZipArchive zipArchive)
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