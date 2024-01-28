using System.IO;
using System.IO.Abstractions;
using System.Threading.Tasks;
using McKinley.ProjectZomboid.Backups.Abstractions.Models;

namespace McKinley.ProjectZomboid.Backups.Abstractions;

public interface IBackupService
{
    Task BackupAsync(Save save, Stream destination);
    Task BackupAsync(Save save, IFileInfo destination);
    
    Task RestoreAsync(IFileInfo source, IDirectoryInfo destination);
    Task RestoreAsync(Stream source, IDirectoryInfo destination);
}