using System.Threading.Tasks;
using McKinley.ProjectZomboid.Backups.Abstractions.Models;

namespace McKinley.ProjectZomboid.Backups.Abstractions;

public interface IBackupService
{
    Task BackupAsync(Save save);
}