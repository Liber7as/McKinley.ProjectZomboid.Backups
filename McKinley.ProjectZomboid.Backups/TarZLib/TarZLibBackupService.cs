using System;
using System.Collections.Generic;
using System.Formats.Tar;
using System.IO;
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
    private readonly IFileSystem _fileSystem;
    private readonly ILogger<TarZLibBackupService>? _logger;

    public TarZLibBackupService(CompressionSettings compressionSettings, IFileSystem fileSystem, ILogger<TarZLibBackupService>? logger = null)
    {
        _compressionSettings = compressionSettings;
        _fileSystem = fileSystem;
        _logger = logger;
    }

    public async Task BackupAsync(Save save, Stream destination)
    {
        _logger?.LogInformation($"Backing up save: '{save.FullName}'");

        await using (var zlibWriter = new ZLibStream(destination, _compressionSettings.CompressionLevel, true))
        await using (var tarWriter = new TarWriter(zlibWriter, TarEntryFormat.Pax))
        {
            _logger?.LogInformation("Beginning file backup");

            foreach (var saveFile in save.Files)
            {
                var entryName = _fileSystem.Path.Combine(save.Name, saveFile.RelativeName);

                _logger?.LogDebug($"'{saveFile.FullName}' -> '{entryName}'");

                var tarEntry = new PaxTarEntry(TarEntryType.RegularFile, entryName)
                {
                    DataStream = saveFile.File.OpenRead()
                };

                await tarWriter.WriteEntryAsync(tarEntry);
            }
        }

        _logger?.LogInformation("Backup written.");
    }

    public async Task BackupAsync(Save save, IFileInfo destination)
    {
        if (destination.Exists)
        {
            throw new ArgumentException("This file already exists.", nameof(destination));
        }

        await using (var destinationStream = destination.Create())
        {
            await BackupAsync(save, destinationStream);
        }

        _logger?.LogInformation($"File saved: '{destination.FullName}'");
    }

    public async Task RestoreAsync(IFileInfo source, IDirectoryInfo destination)
    {
        _logger?.LogInformation($"Restoring backup '{source.FullName}' -> '{destination.FullName}'");

        if (!source.Exists)
        {
            throw new ArgumentException("Backup source does not exist.", nameof(source));
        }

        await using var backupFileStream = source.OpenRead();
        await RestoreAsync(backupFileStream, destination);
    }

    public async Task RestoreAsync(Stream source, IDirectoryInfo destination)
    {
        if (destination.Exists)
        {
            throw new ArgumentException("Backup destination already exists", nameof(destination));
        }

        destination.Create();

        await using var zlibReader = new ZLibStream(source, CompressionMode.Decompress, true);
        await using var tarReader = new TarReader(zlibReader);
        await CopyEntriesToFileSystemAsync(tarReader, destination);
    }

    private async Task CopyEntriesToFileSystemAsync(TarReader tarReader, IDirectoryInfo destination)
    {
        // We can async copy the copy files to the file system
        var tasks = new List<Task>();

        await foreach (var entry in tarReader.GetEntriesAsync())
        {
            var entryDestination = _fileSystem.Path.Combine(destination.FullName, entry.Name);
            var entryDestinationFileInfo = _fileSystem.FileInfo.New(entryDestination);

            tasks.Add(Task.Run(() => CopyEntryToFileSystemAsync(entry, entryDestinationFileInfo)));
        }

        // Ensure the whole archive gets extracted
        await Task.WhenAll(tasks);
    }

    private async Task CopyEntryToFileSystemAsync(TarEntry entry, IFileInfo destination)
    {
        try
        {
            if (entry.DataStream == null)
            {
                _logger?.LogWarning($"Could not find entry '{entry.Name}'");
                return;
            }

            if (destination.Directory == null)
            {
                _logger?.LogWarning($"Could not figure out how to create file '{destination.FullName}'");
                return;
            }

            if (!destination.Directory.Exists)
            {
                destination.Directory.Create();
            }

            _logger?.LogInformation($"'{entry.Name}' -> '{destination.FullName}'");

            await using var destinationStream = destination.Create();

            await entry.DataStream.CopyToAsync(destinationStream);

            // Project Zomboid saves can be large, so lets make sure we get rid of the file loaded into memory:
            await entry.DataStream.DisposeAsync();
        }
        catch (Exception e)
        {
            _logger?.LogCritical(e, e.ToString());

            if (entry.DataStream != null)
            {
                await entry.DataStream.DisposeAsync();
            }

            throw;
        }
    }
}