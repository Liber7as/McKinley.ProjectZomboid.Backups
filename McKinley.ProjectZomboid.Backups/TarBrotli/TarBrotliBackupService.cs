using System;
using System.IO;
using System.IO.Abstractions;
using System.IO.Compression;
using System.Threading.Tasks;
using McKinley.ProjectZomboid.Backups.Abstractions.Models;
using McKinley.ProjectZomboid.Backups.Settings;
using McKinley.ProjectZomboid.Backups.Tar;
using Microsoft.Extensions.Logging;

namespace McKinley.ProjectZomboid.Backups.TarBrotli;

public class TarBrotliBackupService : ITarBrotliBackupService
{
    private readonly CompressionSettings _compressionSettings;
    private readonly ILogger<TarBrotliBackupService>? _logger;
    private readonly ITarBackupService _tarBackupService;

    public TarBrotliBackupService(ITarBackupService tarBackupService, CompressionSettings compressionSettings, ILogger<TarBrotliBackupService>? logger = null)
    {
        _tarBackupService = tarBackupService;
        _compressionSettings = compressionSettings;
        _logger = logger;
    }

    public async Task BackupAsync(Save save, Stream destination)
    {
        _logger?.LogInformation($"Creating Tar file from save: {save.FullName}");

        var tarFileBytes = await GetTarFileAsync(save);
        var compressedBytes = new byte[tarFileBytes.Length];

        var quality = GetCompressionQuality();
        var window = GetCompressionWindow();

        _logger?.LogInformation("Compressing Tar file using Brotli...");

        if (!BrotliEncoder.TryCompress(tarFileBytes, compressedBytes, out var bytesWritten, quality, window))
        {
            throw new ArgumentException("Could not compress the Tar file using Brotli", nameof(save));
        }

        await destination.WriteAsync(compressedBytes, 0, bytesWritten);

        _logger?.LogInformation("Backup written.");
    }

    public async Task BackupAsync(Save save, IFileInfo destination)
    {
        if (destination.Exists)
        {
            throw new ArgumentException("This file already exists.", nameof(destination));
        }

        await using (Stream fileStream = destination.Create())
        {
            await BackupAsync(save, fileStream);
        }

        _logger?.LogInformation($"File saved: '{destination.FullName}'");
    }

    public Task RestoreAsync(IFileInfo source, IDirectoryInfo destination)
    {
        throw new NotImplementedException();
    }

    public Task RestoreAsync(Stream source, IDirectoryInfo destination)
    {
        throw new NotImplementedException();
    }

    private async Task<byte[]> GetTarFileAsync(Save save)
    {
        await using var memoryStream = new MemoryStream();
        await _tarBackupService.BackupAsync(save, memoryStream);
        return memoryStream.ToArray();
    }

    private int GetCompressionQuality()
    {
        // See System.IO.Compression.BrotliUtils
        const int minQuality = 0;
        const int defaultQuality = 4;
        const int maxQuality = 11;

        return _compressionSettings.CompressionLevel switch
        {
            CompressionLevel.Optimal => defaultQuality,
            CompressionLevel.Fastest => defaultQuality,
            CompressionLevel.NoCompression => minQuality,
            CompressionLevel.SmallestSize => maxQuality,
            _ => throw new ArgumentOutOfRangeException("Compression level not supported.")
        };
    }

    private int GetCompressionWindow()
    {
        // See System.IO.Compression.BrotliUtils
        const int minWindow = 10;
        const int defaultWindow = 22;
        const int maxWindow = 24;

        return _compressionSettings.CompressionLevel switch
        {
            CompressionLevel.Optimal => defaultWindow,
            CompressionLevel.Fastest => defaultWindow,
            CompressionLevel.NoCompression => minWindow,
            CompressionLevel.SmallestSize => maxWindow,
            _ => throw new ArgumentOutOfRangeException("Compression level not supported.")
        };
    }
}