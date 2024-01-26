using System;
using System.Formats.Tar;
using System.IO.Abstractions;
using System.IO.Compression;
using System.Threading.Tasks;
using McKinley.ProjectZomboid.Backups.Abstractions.Models;
using McKinley.ProjectZomboid.Backups.Settings;
using Microsoft.Extensions.Logging;

namespace McKinley.ProjectZomboid.Backups.TarZLib;

public class TarZLibBackupService : ITarZLibBackupService
{
    private readonly CompressionSettings _compressionSettings;
    private readonly ILogger<TarZLibBackupService>? _logger;

    public TarZLibBackupService(CompressionSettings compressionSettings, ILogger<TarZLibBackupService>? logger = null)
    {
        _compressionSettings = compressionSettings;
        _logger = logger;
    }

    public async Task BackupAsync(Save save, IFileInfo destination)
    {
        if (destination.Exists)
        {
            throw new ArgumentException("This file already exists.", nameof(destination));
        }

        _logger?.LogInformation($"Backing up save: '{save.FullName}'");

        await using (var backupFileStream = destination.Create())
        await using (var zlibWriter = new ZLibStream(backupFileStream, _compressionSettings.CompressionLevel))
        await using (var tarWriter = new TarWriter(zlibWriter, TarEntryFormat.Pax))
        {
            _logger?.LogInformation("Beginning file backup");

            foreach (var saveFile in save.Files)
            {
                _logger?.LogDebug($"'{saveFile.FullName}' -> '{saveFile.RelativeName}'");

                var tarEntry = new PaxTarEntry(TarEntryType.RegularFile, saveFile.RelativeName)
                {
                    DataStream = saveFile.File.OpenRead()
                };

                await tarWriter.WriteEntryAsync(tarEntry);
            }

            _logger?.LogInformation($"Saving file: '{destination.FullName}'");
        }

        _logger?.LogInformation($"File saved: '{destination.FullName}'");
    }
}