using System.IO.Abstractions;
using McKinley.ProjectZomboid.Backups.Abstractions;
using McKinley.ProjectZomboid.Backups.Abstractions.Models;
using McKinley.ProjectZomboid.Backups.TarZLib;
using McKinley.ProjectZomboid.Backups.Zip;
using Microsoft.Extensions.Logging;

namespace McKinley.ProjectZomboid.Backups.Runner;

public class BackupJob
{
    private readonly IFileSystem _fileSystem;
    private readonly ILogger<BackupJob>? _logger;
    private readonly ISaveService _saveService;
    private readonly ITarZLibBackupService _tarZLibBackupService;
    private readonly IZipBackupService _zipBackupService;

    public BackupJob(ISaveService saveService,
                     IZipBackupService zipBackupService,
                     ITarZLibBackupService tarZLibBackupService,
                     IFileSystem fileSystem,
                     ILogger<BackupJob>? logger = null)
    {
        _saveService = saveService;
        _zipBackupService = zipBackupService;
        _tarZLibBackupService = tarZLibBackupService;
        _fileSystem = fileSystem;
        _logger = logger;
    }

    public async Task<int> RunAsync(CommandLineArgumentsModel args)
    {
        var saveDirectory = GetSaveDirectory(args);

        if (saveDirectory == null)
        {
            _logger?.LogError("Could not find the Project Zomboid save directory.");

            return -1;
        }

        var saves = (await _saveService.GetAsync(saveDirectory)).ToArray();

        if (args.SaveName != null)
        {
            _logger?.LogInformation($"Finding saves with name '{args.SaveName}'");

            saves = saves.Where(save => string.Equals(save.Name, args.SaveName, StringComparison.OrdinalIgnoreCase))
                         .ToArray();

            _logger?.LogInformation($"Found {saves.Length} saves with name '{args.SaveName}'");
        }

        foreach (var save in saves)
        {
            switch (args.BackupType)
            {
                case BackupType.Zip:
                    await ZipBackup(save, args);
                    break;
                case BackupType.TarZLib:
                    await TarZLibBackup(save, args);
                    break;
                default:
                    throw new NotSupportedException("Backup type not supported.");
            }
        }

        return 0;
    }

    private Task ZipBackup(Save save, CommandLineArgumentsModel args)
    {
        var backupFileName = Path.Combine(args.OutputFolder, args.ZipFileName);
        var backupFileInfo = _fileSystem.FileInfo.New(backupFileName);

        return _zipBackupService.BackupAsync(save, backupFileInfo);
    }

    private Task TarZLibBackup(Save save, CommandLineArgumentsModel args)
    {
        var uniqueTimestamp = DateTime.UtcNow.ToString("s")
                                      .Replace(":", string.Empty)
                                      .Replace("-", string.Empty)
                                      .Replace("T", string.Empty);

        var backupFileName = Path.Combine(args.OutputFolder, $"{save.Name}-Backup-{uniqueTimestamp}.tar.zl");
        var backupFileInfo = _fileSystem.FileInfo.New(backupFileName);

        return _tarZLibBackupService.BackupAsync(save, backupFileInfo);
    }

    private IDirectoryInfo? GetSaveDirectory(CommandLineArgumentsModel args)
    {
        var directory = _fileSystem.DirectoryInfo.New(args.SaveDirectory);

        _logger?.LogInformation($"Project Zomboid save directory: '{directory.FullName}'");

        // If the save directory doesn't exist, return null to indicate it could not be found.
        return !directory.Exists
                   ? null
                   : directory;
    }
}