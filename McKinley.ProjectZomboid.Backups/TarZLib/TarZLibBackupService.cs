using System;
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
        if (destination.Exists)
        {
            throw new ArgumentException("", nameof(destination));
        }

        throw new NotImplementedException();
    }

    private Task CopyFileToTarArchiveAsync(TarWriter tarWriter, string entryName, SaveFile saveFile)
    {
        var entry = new PaxTarEntry(TarEntryType.RegularFile, entryName)
        {
            DataStream = saveFile.File.OpenRead()
        };

        _logger?.LogDebug($"'{saveFile.FullName}' -> '{entry.Name}'");

        return tarWriter.WriteEntryAsync(entry);
    }

    private TarWriter CreateTarZLibWriter(Stream output, bool leaveOpen = false)
    {
        var zlibWriter = new ZLibStream(output, _settings.CompressionLevel, leaveOpen);
        var tarWriter = new TarWriter(zlibWriter, false);

        return tarWriter;
    }
}