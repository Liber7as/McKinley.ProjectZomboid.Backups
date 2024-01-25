using CommandLine;
using Microsoft.Extensions.Logging;

namespace McKinley.ProjectZomboid.Backups.Runner;

public class RunnerSettings
{
    [Option('f', "file", Default = "ProjectZomboid-Backups.zip", HelpText = "Backup Zip File Location. If not provided, a ZIP file will be created next to your saves.")]
    public string BackupZipFileLocation { get; set; } = "ProjectZomboid-Backups.zip";

    [Option('d', "directory", HelpText = "Project Zomboid Saves Directory. If not provided, it will try to detect your default Project Zomboid saves directory (ex: C:\\Users\\{Your Account Name}\\Zomboid\\Saves\\Sandbox).")]
    public string? SaveDirectory { get; set; }

    [Option("log-level", Default = LogLevel.Information, HelpText = "The log level to output to the console.")]
    public LogLevel LogLevel { get; set; } = LogLevel.Information;
}