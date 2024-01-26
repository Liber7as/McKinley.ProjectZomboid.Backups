using CommandLine;
using Microsoft.Extensions.Logging;

namespace McKinley.ProjectZomboid.Backups.Runner;

public class CommandLineArgumentsModel
{
    [Option('t', "type", HelpText = "Backup file type. Zip and TarZLib are supported.")]
    public BackupType BackupType { get; set; } = BackupType.Zip;

    [Option('o', "output", HelpText = "Backup output folder. By default the current directory will be used.")]
    public string OutputFolder { get; set; } = Environment.CurrentDirectory;

    [Option('s', "save-directory", HelpText = "Project Zomboid Saves Directory. If not provided, it will try to detect your default Project Zomboid saves directory (ex: %USERPROFILE%\\Zomboid\\Saves\\Sandbox).")]
    public string SaveDirectory { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), @"Zomboid\Saves\Sandbox");

    [Option("save-name", HelpText = "Only back up this save, ignores all others.")]
    public string? SaveName { get; set; }

    [Option("log-level", Default = LogLevel.Information, HelpText = "The log level to output to the console.")]
    public LogLevel LogLevel { get; set; } = LogLevel.Information;

    [Option("zip-filename", HelpText = "Zip backup file name")]
    public string ZipFileName { get; set; } = "ProjectZomboid-Backups.zip";
}