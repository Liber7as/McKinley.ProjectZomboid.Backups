using System.IO.Abstractions;

namespace McKinley.ProjectZomboid.Backups.Abstractions.Models;

public class SaveFile
{
    public SaveFile(Save save, IFileInfo file)
    {
        Save = save;
        File = file;
    }

    public Save Save { get; }

    public IFileInfo File { get; }

    public string FullName => File.FullName;

    /// <summary>
    /// The name of the file relative to the save directory.
    /// </summary>
    public string RelativeName => File.FullName.Replace(Save.Directory.FullName, string.Empty)
                                      .TrimStart('\\', '/');
}