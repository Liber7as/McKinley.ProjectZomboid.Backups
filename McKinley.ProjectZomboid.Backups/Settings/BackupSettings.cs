using System.IO.Compression;

namespace McKinley.ProjectZomboid.Backups.Settings;

public class BackupSettings
{
    public string BackupNameFormat { get; set; } = "{0}-Backup-{1}";

    public CompressionLevel CompressionLevel { get; set; } = CompressionLevel.SmallestSize;
}