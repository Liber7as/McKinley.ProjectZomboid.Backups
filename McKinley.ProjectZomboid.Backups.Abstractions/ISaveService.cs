using System.Collections.Generic;
using System.IO.Abstractions;
using System.Threading.Tasks;
using McKinley.ProjectZomboid.Backups.Abstractions.Models;

namespace McKinley.ProjectZomboid.Backups.Abstractions;

public interface ISaveService
{
    Task<IEnumerable<Save>> GetAsync(IDirectoryInfo saveDirectory);
}