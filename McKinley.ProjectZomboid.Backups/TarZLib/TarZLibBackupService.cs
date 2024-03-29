﻿using System;
using System.Collections.Generic;
using System.Formats.Tar;
using System.IO;
using System.IO.Abstractions;
using System.IO.Compression;
using System.Threading.Tasks;
using McKinley.ProjectZomboid.Backups.Abstractions.Models;
using McKinley.ProjectZomboid.Backups.Settings;
using McKinley.ProjectZomboid.Backups.Tar;
using Microsoft.Extensions.Logging;

namespace McKinley.ProjectZomboid.Backups.TarZLib;

public class TarZLibBackupService : ITarZLibBackupService
{
    private readonly CompressionSettings _compressionSettings;
    private readonly ITarBackupService _tarBackupService;
    private readonly ILogger<TarZLibBackupService>? _logger;

    public TarZLibBackupService(CompressionSettings compressionSettings, ITarBackupService tarBackupService, ILogger<TarZLibBackupService>? logger = null)
    {
        _compressionSettings = compressionSettings;
        _tarBackupService = tarBackupService;
        _logger = logger;
    }

    public async Task BackupAsync(Save save, Stream destination)
    {
        _logger?.LogInformation($"Backing up save: '{save.FullName}'");

        await using (var zlibWriter = new ZLibStream(destination, _compressionSettings.CompressionLevel, true))
        {
            await _tarBackupService.BackupAsync(save, zlibWriter);
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

        await using (var backupFileStream = source.OpenRead())
        {
            await RestoreAsync(backupFileStream, destination);
        }
        
        _logger?.LogInformation($"Backup restored '{destination.FullName}'");
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
            var entryDestination = destination.FileSystem.Path.Combine(destination.FullName, entry.Name);
            var entryDestinationFileInfo = destination.FileSystem.FileInfo.New(entryDestination);

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

            _logger?.LogDebug($"'{entry.Name}' -> '{destination.FullName}'");

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