using System;
using System.IO;
using System.IO.Abstractions;
using System.IO.Compression;
using System.Threading.Tasks;
using McKinley.ProjectZomboid.Backups.Abstractions.Models;
using McKinley.ProjectZomboid.Backups.Settings;
using Microsoft.Extensions.Logging;

namespace McKinley.ProjectZomboid.Backups.Zip;

public class ZipBackupService : IZipBackupService
{
    private readonly ILogger<ZipBackupService>? _logger;
    private readonly CompressionSettings _compressionSettings;

    public ZipBackupService(CompressionSettings compressionSettings, ILogger<ZipBackupService>? logger = null)
    {
        _compressionSettings = compressionSettings;
        _logger = logger;
    }

    public async Task BackupAsync(Save save, Stream destination)
    {
        _logger?.LogInformation($"Backing up save: '{save.FullName}'");

        // Determine if we should create or update a ZIP file.
        var zipArchiveMode = destination.CanSeek && destination.Length > 0
                              ? ZipArchiveMode.Update
                              : ZipArchiveMode.Create;

        _logger?.LogInformation(zipArchiveMode == ZipArchiveMode.Update
                                    ? "Updating ZIP file"
                                    : "Creating ZIP file");

        _logger?.LogInformation("Beginning file backup");

        // Create a zip archive from the stream above, and copy the save to it.
        using (var zipArchive = new ZipArchive(destination, zipArchiveMode, true))
        {

            // Enumerate all the files in the save
            foreach (var saveFile in save.Files)
            {
                // Create an entry name for the file
                var entryName = save.FileSystem.Path.Combine(save.Name, saveFile.RelativeName);

                await CopyFileToZipArchiveAsync(zipArchive, entryName, saveFile);
            }

            _logger?.LogInformation("Saving zip file...");
        }

        _logger?.LogInformation("Backup written.");
    }

    public async Task BackupAsync(Save save, IFileInfo destination)
    {
        // Check to see if the ZIP file already exists
        _logger?.LogInformation(destination.Exists
                                    ? $"Found backup zip file: '{destination.FullName}'"
                                    : $"Backup zip file not found. Will create: '{destination.FullName}'");

        // Open the ZIP file as a stream, or create a new file. 
        await using (Stream zipFileStream = destination.Exists
                                                ? destination.Open(FileMode.Open, FileAccess.ReadWrite)
                                                : destination.Create())
        {
            await BackupAsync(save, zipFileStream);
        }

        _logger?.LogInformation($"File saved: '{destination.FullName}'");
    }

    public Task RestoreAsync(IFileInfo source, IDirectoryInfo destination)
    {
        throw new NotImplementedException();
    }

    public Task RestoreAsync(Stream source, IDirectoryInfo destination)
    {
        throw new NotImplementedException();
    }

    private async Task CopyFileToZipArchiveAsync(ZipArchive zipArchive, string entryName, SaveFile saveFile)
    {
        var entry = zipArchive.CreateEntry(entryName, _compressionSettings.CompressionLevel);

        _logger?.LogDebug($"'{saveFile.FullName}' -> '{entry.FullName}'");

        // Open the zip file entry and the file
        await using var zipEntryStream = entry.Open();
        await using var fileStream = saveFile.File.OpenRead();

        // Copy the file to the ZIP archive
        await fileStream.CopyToAsync(zipEntryStream);
    }
}