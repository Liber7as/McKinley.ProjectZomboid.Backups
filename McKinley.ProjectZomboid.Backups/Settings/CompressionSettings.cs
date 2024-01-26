using System.IO.Compression;

namespace McKinley.ProjectZomboid.Backups.Settings;

public class CompressionSettings
{
    public CompressionLevel CompressionLevel { get; set; } = CompressionLevel.SmallestSize;
}