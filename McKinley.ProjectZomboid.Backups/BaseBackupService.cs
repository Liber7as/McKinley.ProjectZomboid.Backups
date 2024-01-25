using System;
using System.IO;
using System.IO.Abstractions;
using System.Threading.Tasks;
using McKinley.ProjectZomboid.Backups.Settings;

namespace McKinley.ProjectZomboid.Backups;

public abstract class BaseBackupService
{
    private readonly BackupSettings _settings;

    protected BaseBackupService(BackupSettings settings)
    {
        _settings = settings;
    }

    protected async Task EnumerateFilesAsync(IDirectoryInfo directoryInfo, Func<string, IFileInfo, Task> forEachFileAsync)
    {
        // Create a unique timestamp for the backup
        var uniqueDateString = DateTime.UtcNow.ToString("s")
                                       .Replace(":", string.Empty)
                                       .Replace("-", string.Empty)
                                       .Replace("T", string.Empty);

        // Create a folder name for the ZIP file backup
        var folderName = string.Format(_settings.BackupNameFormat, directoryInfo.Name, uniqueDateString);

        // Enumerate all the files in the save
        foreach (var file in directoryInfo.GetFiles("*", SearchOption.AllDirectories))
        {
            // Create a relative path for the file being imported into the ZIP file
            var relativeFileName = file.FullName.Replace(directoryInfo.FullName, string.Empty)
                                       .TrimStart('\\')
                                       .TrimStart('/');

            // Create an entry name for the file
            var entryPath = Path.Combine(folderName, relativeFileName);

            await forEachFileAsync(entryPath, file);
        }
    }
}