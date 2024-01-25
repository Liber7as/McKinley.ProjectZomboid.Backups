using System.IO.Abstractions;
using McKinley.ProjectZomboid.Backups.Abstractions;
using Microsoft.Extensions.Logging;

namespace McKinley.ProjectZomboid.Backups.Runner;

public class BackupJob
{
    private readonly IBackupService _backupService;
    private readonly IFileSystem _fileSystem;
    private readonly ILogger<BackupJob>? _logger;
    private readonly ISaveService _saveService;
    private readonly RunnerSettings _settings;

    public BackupJob(ISaveService saveService,
                     IBackupService backupService,
                     IFileSystem fileSystem,
                     RunnerSettings settings,
                     ILogger<BackupJob>? logger = null)
    {
        _saveService = saveService;
        _backupService = backupService;
        _fileSystem = fileSystem;
        _settings = settings;
        _logger = logger;
    }

    public async Task<int> RunAsync()
    {
        var saveDirectory = GetSaveDirectory();

        if (saveDirectory == null)
        {
            _logger?.LogError("Could not find the Project Zomboid save directory.");

            return -1;
        }

        var saves = await _saveService.GetAsync(saveDirectory);

        foreach (var save in saves)
        {
            await _backupService.BackupAsync(save);
        }

        return 0;
    }

    private IDirectoryInfo? GetSaveDirectory()
    {
        var directory = _fileSystem.DirectoryInfo.New(!string.IsNullOrWhiteSpace(_settings.SaveDirectory)
                                                          ? _settings.SaveDirectory // If the user provided a save directory, we can use that.
                                                          : @"%USERPROFILE%\Zomboid\Saves\Sandbox"); // Attempt to locate the save directory

        // If the save directory doesn't exist, return null to indicate it could not be found.
        return !directory.Exists
                   ? null
                   : directory;
    }
}