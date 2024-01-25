using System.IO;

namespace McKinley.ProjectZomboid.Backups.Abstractions.Models;

public class Save
{
    public Save(DirectoryInfo directory)
    {
        Directory = directory;
    }

    public DirectoryInfo Directory { get; }

    public string Name => Directory.Name;
}