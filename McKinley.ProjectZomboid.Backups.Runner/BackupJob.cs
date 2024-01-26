﻿using System.IO.Abstractions;
using McKinley.ProjectZomboid.Backups.Abstractions;
using McKinley.ProjectZomboid.Backups.Abstractions.Models;
using Microsoft.Extensions.Logging;

namespace McKinley.ProjectZomboid.Backups.Runner;

public class BackupJob
{
    private readonly IBackupService _backupService;
    private readonly IFileSystem _fileSystem;
    private readonly ILogger<BackupJob>? _logger;
    private readonly ISaveService _saveService;

    public BackupJob(ISaveService saveService,
                     IBackupService backupService,
                     IFileSystem fileSystem,
                     ILogger<BackupJob>? logger = null)
    {
        _saveService = saveService;
        _backupService = backupService;
        _fileSystem = fileSystem;
        _logger = logger;
    }

    public async Task<int> RunAsync(CommandLineArgumentsModel args)
    {
        var saveDirectory = GetSaveDirectory();

        if (saveDirectory == null)
        {
            _logger?.LogError("Could not find the Project Zomboid save directory.");

            return -1;
        }

        var saves = (await _saveService.GetAsync(saveDirectory)).ToArray();

        if (args.SaveName != null)
        {
            _logger?.LogInformation($"Finding saves with name '{args.SaveName}'");

            saves = saves.Where(save => string.Equals(save.Name, args.SaveName, StringComparison.OrdinalIgnoreCase)).ToArray();

            _logger?.LogInformation($"Found {saves.Length} saves with name '{args.SaveName}'");
        }

        foreach (var save in saves)
        {
            switch (args.BackupType)
            {
                case BackupType.Zip:
                    await ZipBackup(save);
                    break;
                case BackupType.TarZLib:
                    await TarZLibBackup(save);
                    break;
                default:
                    throw new NotSupportedException("Backup type not supported.");
            }
        }

        return 0;
    }

    private Task ZipBackup(Save save)
    {
        var backupFileName = Path.Combine(_settings.OutputFolder, _settings.ZipFileName);
        var backupFileInfo = _fileSystem.FileInfo.New(backupFileName);

        return _backupService.BackupAsync(save, backupFileInfo);
    }

    private Task TarZLibBackup(Save save)
    {
        var uniqueTimestamp = DateTime.UtcNow.ToString("s")
                                      .Replace(":", string.Empty)
                                      .Replace("-", string.Empty)
                                      .Replace("T", string.Empty);

        var backupFileName = Path.Combine(_settings.OutputFolder, $"{save.Name}-Backup-{uniqueTimestamp}.tar.zl");
        var backupFileInfo = _fileSystem.FileInfo.New(backupFileName);

        return _backupService.BackupAsync(save, backupFileInfo);
    }

    private IDirectoryInfo? GetSaveDirectory()
    {
        var directory = _fileSystem.DirectoryInfo.New(_settings.SaveDirectory);

        _logger?.LogInformation($"Project Zomboid save directory: '{directory.FullName}'");

        // If the save directory doesn't exist, return null to indicate it could not be found.
        return !directory.Exists
                   ? null
                   : directory;
    }
}