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

        await using (var tarWriter = await CreateTarZLibWriterAsync(destination))
        {
            await EnumerateFilesAsync(save.Directory, (entryName, fileInfo) => CopyFileToTarArchiveAsync(tarWriter, entryName, fileInfo));

            _logger?.LogInformation("Completed file backup.");

            _logger?.LogInformation($"Saving tar/zlib file: '{destination.FullName}'");
        }

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

    private async Task<TarWriter> CreateTarZLibWriterAsync(IFileInfo destination)
    {
        // If the destination doesn't exist, we can create it from scratch:
        if (!destination.Exists)
        {
            return CreateTarZLibWriter(destination.Create());
        }

        // If the destination exists

        // Create a temporary file
        var temporaryFileInfo = destination.FileSystem.FileInfo.New(destination.FullName + ".tmp");

        // Copy the existing backup to the temporary file
        destination.CopyTo(temporaryFileInfo.FullName, true);

        // Overwrite the existing backup
        var tarWriter = CreateTarZLibWriter(destination.Create());

        // Copy the temporary file to the new backup
        await using var content = temporaryFileInfo.OpenRead();
        await using var tarReader = CreateTarZLibReader(content);
        TarEntry? entry;
        while ((entry = await tarReader.GetNextEntryAsync()) != null)
        {
            await tarWriter.WriteEntryAsync(entry);
        }

        // Return the new backup writer
        return tarWriter;
    }

    private TarReader CreateTarZLibReader(Stream input, bool leaveOpen = false)
    {
        var zlibReader = new ZLibStream(input, _settings.CompressionLevel, leaveOpen);
        var tarReader = new TarReader(zlibReader);

        return tarReader;
    }

    private TarWriter CreateTarZLibWriter(Stream output, bool leaveOpen = false)
    {
        var zlibWriter = new ZLibStream(output, _settings.CompressionLevel, leaveOpen);
        var tarWriter = new TarWriter(zlibWriter, false);

        return tarWriter;
    }
}