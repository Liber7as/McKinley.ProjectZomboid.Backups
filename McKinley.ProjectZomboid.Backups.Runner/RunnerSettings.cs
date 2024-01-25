using CommandLine;
using CommandLine.Text;
using Microsoft.Extensions.Logging;

namespace McKinley.ProjectZomboid.Backups.Runner;

public class RunnerSettings
{
    [Option('f', "file", HelpText = "Backup Zip File Location")]
    public string? BackupZipFileLocation { get; set; }

    [Option('d', "directory", HelpText = "Project Zomboid Saves Directory", Required = true)]
    public string SaveDirectory { get; set; } = "";

    [Option("log-level", Default = LogLevel.Information, HelpText = "The log level to output to the console.")]
    public LogLevel LogLevel { get; set; } = LogLevel.Information;
}