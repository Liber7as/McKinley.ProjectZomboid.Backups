using System.IO.Compression;

namespace McKinley.ProjectZomboid.Backups.Zip.Settings;

public class ZipBackupSettings
{
    public string FileLocation { get; set; } = "ProjectZomboid-Backups.zip";

    public string BackupNameFormat { get; set; } = "{0}-Backup-{1}";

    public CompressionLevel CompressionLevel { get; set; } = CompressionLevel.SmallestSize;
}