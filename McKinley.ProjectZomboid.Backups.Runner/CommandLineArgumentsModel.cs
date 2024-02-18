using System.IO.Compression;
using CommandLine;
using Microsoft.Extensions.Logging;

namespace McKinley.ProjectZomboid.Backups.Runner;

public class CommandLineArgumentsModel
{
    [Option('t', "type", HelpText = "The backup file type. Zip, TarZLib, and TarBrotli are supported. The default is Zip")]
    public BackupType BackupType { get; set; } = BackupType.Zip;

    [Option('o', "output", HelpText = "Backup output folder. By default, the current directory will be used.")]
    public string OutputFolder { get; set; } = Environment.CurrentDirectory;

    [Option('s', "save-directory", HelpText = "Project Zomboid Saves Directory. By default, it will try to detect your default Project Zomboid saves directory (ex: %USERPROFILE%\\Zomboid\\Saves\\Sandbox).")]
    public string SaveDirectory { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), @"Zomboid\Saves\Sandbox");

    [Option("save-name", HelpText = "Only back up this save, ignores all others. There is no default value.")]
    public string? SaveName { get; set; }

    [Option("log-level", HelpText = "The log level to output to the console (Trace/Debug/Information/Warning/Error/Critical/None). The default is Information.")]
    public LogLevel LogLevel { get; set; } = LogLevel.Information;

    [Option("compression-level", HelpText = "Compression Level (Optimal/Fastest/NoCompression/SmallestSize). The default is Optimal.")]
    public CompressionLevel CompressionLevel { get; set; } = CompressionLevel.Optimal;
}