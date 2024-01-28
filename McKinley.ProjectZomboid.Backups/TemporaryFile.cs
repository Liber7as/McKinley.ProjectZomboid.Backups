using System;
using System.IO;
using System.IO.Abstractions;

namespace McKinley.ProjectZomboid.Backups;

internal class TemporaryFile : IDisposable
{
    private readonly IFileInfo _file;

    internal TemporaryFile(IFileInfo file)
    {
        _file = file;
    }

    internal TemporaryFile(IFileSystem fileSystem)
    {
        _file = fileSystem.FileInfo.New(fileSystem.Path.GetTempFileName());
    }

    public FileSystemStream Open()
    {
        return _file.Exists
                   ? _file.Create()
                   : _file.Open(FileMode.Open);
    }

    public void Dispose()
    {
        if (!_file.Exists)
        {
            return;
        }

        _file.Delete();
    }
}