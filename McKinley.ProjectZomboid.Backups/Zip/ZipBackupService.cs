using System.IO;
using System.IO.Abstractions;
using System.IO.Compression;
using System.Threading.Tasks;
using McKinley.ProjectZomboid.Backups.Abstractions.Models;
using McKinley.ProjectZomboid.Backups.Settings;
using Microsoft.Extensions.Logging;

namespace McKinley.ProjectZomboid.Backups.Zip;

public class ZipBackupService : BaseBackupService,
                                IZipBackupService
{
    private readonly ILogger<ZipBackupService>? _logger;
    private readonly BackupSettings _settings;

    public ZipBackupService(BackupSettings settings, ILogger<ZipBackupService>? logger = null)
        : base(settings)
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
                                               : destination.Create();

        // Determine if we should create or update a ZIP file.
        var zipArchiveMode = destination.Exists
                                 ? ZipArchiveMode.Update
                                 : ZipArchiveMode.Create;

        _logger?.LogInformation("Beginning file backup...");

        // Create a zip archive from the stream above, and copy the save to it.
        using (var zipArchive = new ZipArchive(zipFileStream, zipArchiveMode, true))
        {
            await EnumerateFilesAsync(save, (entryName, saveFile) => CopyFileToZipArchiveAsync(zipArchive, entryName, saveFile));

            _logger?.LogInformation("Completed file backup.");

            _logger?.LogInformation($"Saving zip file: '{destination.FullName}'");
        }

        await zipFileStream.FlushAsync();

        _logger?.LogInformation($"Zip file saved: '{destination.FullName}'");
    }

    private async Task CopyFileToZipArchiveAsync(ZipArchive zipArchive, string entryName, SaveFile saveFile)
    {
        var entry = zipArchive.CreateEntry(entryName, _settings.CompressionLevel);

        _logger?.LogDebug($"'{saveFile.FullName}' -> '{entry.FullName}'");

        // Open the zip file entry and the file
        await using var zipEntryStream = entry.Open();
        await using var fileStream = saveFile.File.OpenRead();

        // Copy the file to the ZIP archive
        await fileStream.CopyToAsync(zipEntryStream);

        // Flush to ensure everything is sent
        await fileStream.FlushAsync();
        await zipEntryStream.FlushAsync();
    }
}