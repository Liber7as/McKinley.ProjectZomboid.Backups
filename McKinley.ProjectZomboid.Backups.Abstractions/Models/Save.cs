using System.IO.Abstractions;

namespace McKinley.ProjectZomboid.Backups.Abstractions.Models;

public class Save
{
    public Save(IDirectoryInfo directory)
    {
        Directory = directory;
    }

    public IDirectoryInfo Directory { get; }

    public string Name => Directory.Name;
}