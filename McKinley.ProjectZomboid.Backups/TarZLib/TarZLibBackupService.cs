using System.Formats.Tar;
using System.IO;
using System.IO.Abstractions;
using System.IO.Compression;
using System.Threading.Tasks;
using McKinley.ProjectZomboid.Backups.Abstractions.Models;
using McKinley.ProjectZomboid.Backups.Settings;
using Microsoft.Extensions.Logging;

namespace McKinley.ProjectZomboid.Backups.TarZLib;

public class TarZLibBackupService : BaseBackupService,
                                    ITarZLibBackupService
{
    private readonly ILogger<TarZLibBackupService>? _logger;
    private readonly BackupSettings _settings;

    public TarZLibBackupService(BackupSettings settings, ILogger<TarZLibBackupService>? logger = null)
        : base(settings)
    {
        _settings = settings;
        _logger = logger;
    }

    public async Task BackupAsync(Save save, IFileInfo destination)
    {
        _logger?.LogInformation($"Backing up save: '{save.Directory.FullName}'");

        _logger?.LogInformation(destination.Exists
                                    ? $"Found backup tar/zlib file: '{destination.FullName}'"
                                    : $"Backup tar/zlib file not found. Will create: '{destination.FullName}'");

        await using Stream tarZLibStream = destination.Exists
                                               ? destination.Open(FileMode.Open, FileAccess.Write)
                                               : destination.Create();

        await using (var lzWriter = new ZLibStream(tarZLibStream, _settings.CompressionLevel, true))
        await using (var tarWriter = new TarWriter(lzWriter, TarEntryFormat.Pax, true))
        {
            await EnumerateFilesAsync(save.Directory, (entryName, fileInfo) => CopyFileToTarArchiveAsync(tarWriter, entryName, fileInfo));

            _logger?.LogInformation("Completed file backup.");

            _logger?.LogInformation($"Saving tar/zlib file: '{destination.FullName}'");

            await lzWriter.FlushAsync();
        }

        await tarZLibStream.FlushAsync();

        _logger?.LogInformation($"Tar/zlib file saved: '{destination.FullName}'");
    }

    private Task CopyFileToTarArchiveAsync(TarWriter tarWriter, string entryName, IFileInfo fileInfo)
    {
        var entry = new PaxTarEntry(TarEntryType.RegularFile, entryName)
        {
            DataStream = fileInfo.OpenRead()
        };

        _logger?.LogDebug($"'{fileInfo.FullName}' -> '{entry.Name}'");

        return tarWriter.WriteEntryAsync(entry);
    }
}