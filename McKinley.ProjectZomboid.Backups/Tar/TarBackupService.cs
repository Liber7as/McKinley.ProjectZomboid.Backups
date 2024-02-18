using System;
using System.Formats.Tar;
using System.IO;
using System.IO.Abstractions;
using System.Threading.Tasks;
using McKinley.ProjectZomboid.Backups.Abstractions.Models;
using Microsoft.Extensions.Logging;

namespace McKinley.ProjectZomboid.Backups.Tar;

public class TarBackupService : ITarBackupService
{
    private readonly ILogger<TarBackupService>? _logger;

    public TarBackupService(ILogger<TarBackupService>? logger = null)
    {
        _logger = logger;
    }

    public async Task BackupAsync(Save save, Stream destination)
    {
        await using var tarWriter = new TarWriter(destination, TarEntryFormat.Pax);

        _logger?.LogInformation("Beginning file backup");

        foreach (var saveFile in save.Files)
        {
            var entryName = save.FileSystem.Path.Combine(save.Name, saveFile.RelativeName);

            _logger?.LogDebug($"'{saveFile.FullName}' -> '{entryName}'");

            var tarEntry = new PaxTarEntry(TarEntryType.RegularFile, entryName)
            {
                DataStream = saveFile.File.OpenRead()
            };

            await tarWriter.WriteEntryAsync(tarEntry);
        }
    }

    public Task BackupAsync(Save save, IFileInfo destination)
    {
        throw new NotImplementedException();
    }

    public Task RestoreAsync(IFileInfo source, IDirectoryInfo destination)
    {
        throw new NotImplementedException();
    }

    public Task RestoreAsync(Stream source, IDirectoryInfo destination)
    {
        throw new NotImplementedException();
    }
}