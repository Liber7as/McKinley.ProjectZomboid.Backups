using System.IO.Compression;

namespace McKinley.ProjectZomboid.Backups.Settings;

public class BackupSettings
{
    public CompressionLevel CompressionLevel { get; set; } = CompressionLevel.SmallestSize;
}