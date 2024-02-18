using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;

namespace McKinley.ProjectZomboid.Backups.Abstractions.Models;

public class Save
{
    public Save(IDirectoryInfo directory)
    {
        Directory = directory;
    }

    public IFileSystem FileSystem => Directory.FileSystem;

    public IDirectoryInfo Directory { get; }

    public IEnumerable<SaveFile> Files => Directory.GetFiles("*", SearchOption.AllDirectories).Select(file => new SaveFile(this, file));

    public string Name => Directory.Name;

    public string FullName => Directory.FullName;
}