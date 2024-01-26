using System;
using System.IO;
using System.IO.Abstractions;
using System.Threading.Tasks;
using McKinley.ProjectZomboid.Backups.Abstractions.Models;
using McKinley.ProjectZomboid.Backups.Settings;

namespace McKinley.ProjectZomboid.Backups;

public abstract class BaseBackupService
{
    private readonly BackupSettings _settings;

    protected BaseBackupService(BackupSettings settings)
    {
        _settings = settings;
    }

    /// <summary>
    /// Enumerates the files in a directory and creates a unique entry name, then calls the `forEachFileAsync` parameter with the file and the entry name.
    /// </summary>
    protected async Task EnumerateFilesAsync(Save save, Func<string, SaveFile, Task> forEachFileAsync)
    {
        // Create a unique timestamp for the backup
        var uniqueDateString = DateTime.UtcNow.ToString("s")
                                       .Replace(":", string.Empty)
                                       .Replace("-", string.Empty)
                                       .Replace("T", string.Empty);

        // Create a folder name for the ZIP file backup
        var folderName = string.Format(_settings.BackupNameFormat, save.Name, uniqueDateString);

        // Enumerate all the files in the save
        foreach (var file in save.Files)
        {
            // Create an entry name for the file
            var entryPath = Path.Combine(folderName, file.RelativeName);

            await forEachFileAsync(entryPath, file);
        }
    }
}