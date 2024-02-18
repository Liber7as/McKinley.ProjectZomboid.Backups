using System.IO.Abstractions;
using McKinley.ProjectZomboid.Backups.Abstractions;
using McKinley.ProjectZomboid.Backups.Abstractions.Models;
using McKinley.ProjectZomboid.Backups.TarBrotli;
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
    private readonly ITarBrotliBackupService _tarBrotliBackupService;
    private readonly IZipBackupService _zipBackupService;

    public BackupJob(ISaveService saveService,
                     IZipBackupService zipBackupService,
                     ITarZLibBackupService tarZLibBackupService,
                     ITarBrotliBackupService tarBrotliBackupService,
                     IFileSystem fileSystem,
                     ILogger<BackupJob>? logger = null)
    {
        _saveService = saveService;
        _zipBackupService = zipBackupService;
        _tarZLibBackupService = tarZLibBackupService;
        _tarBrotliBackupService = tarBrotliBackupService;
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
            // Create a unique timestamp in the Project Zomboid format
            var uniqueTimestamp = DateTime.UtcNow.ToString("s")
                                          .Replace(":", "-")
                                          .Replace("T", "_");

            var backupName = $"{save.Name}-Backup-{uniqueTimestamp}";

            string backupFileName;
            IBackupService backupService;

            switch (args.BackupType)
            {
                case BackupType.Zip:
                    backupFileName = backupName + ".zip";
                    backupService = _zipBackupService;
                    break;
                case BackupType.TarZLib:
                    backupFileName = backupName + ".tar.zl";
                    backupService = _tarZLibBackupService;
                    break;
                case BackupType.TarBrotli:
                    backupFileName = backupName + ".tar.br";
                    backupService = _tarBrotliBackupService;
                    break;
                default:
                    throw new NotSupportedException("Backup type not supported.");
            }

            var backupFileInfo = _fileSystem.FileInfo.New(_fileSystem.Path.Combine(args.OutputFolder, backupFileName));
            await backupService.BackupAsync(save, backupFileInfo);
        }

        return 0;
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