using CommandLine;
using McKinley.ProjectZomboid.Backups.Abstractions;
using McKinley.ProjectZomboid.Backups.Zip.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace McKinley.ProjectZomboid.Backups.Runner;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        var settings = ParseSettings(args);

        if (settings == null)
        {
            return -1;
        }

        var serviceProvider = ConfigureServices(settings);

        var saveService = serviceProvider.GetRequiredService<ISaveService>();
        var backupService = serviceProvider.GetRequiredService<IBackupService>();

        foreach (var save in await saveService.GetAsync(new DirectoryInfo(settings.SaveDirectory)))
        {
            await backupService.BackupAsync(save);
        }

        return 0;
    }

    private static ServiceProvider ConfigureServices(RunnerSettings settings)
    {
        var zipBackupSettings = new ZipBackupSettings();

        if (!string.IsNullOrWhiteSpace(settings.BackupZipFileLocation))
        {
            zipBackupSettings.FileLocation = settings.BackupZipFileLocation;
        }

        var services = new ServiceCollection();
        services.AddZipBackups(zipBackupSettings);

        services.AddLogging(loggingBuilder =>
                            {
                                loggingBuilder.ClearProviders();
                                loggingBuilder.AddConsole();
                                loggingBuilder.AddFilter(logLevel => logLevel >= settings.LogLevel);
                            });

        return services.BuildServiceProvider();
    }

    private static RunnerSettings? ParseSettings(string[] args)
    {
        var parsedArgumentsResult = Parser.Default.ParseArguments<RunnerSettings>(args);

        if (!parsedArgumentsResult.Errors.Any())
        {
            return parsedArgumentsResult.Value;
        }

        parsedArgumentsResult.Errors.Output();

        return null;
    }
}