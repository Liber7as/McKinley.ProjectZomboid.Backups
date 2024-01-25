using CommandLine;
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

        await using var serviceProvider = ConfigureServices(settings);

        var backupJob = serviceProvider.GetRequiredService<BackupJob>();

        return await backupJob.RunAsync();
    }

    private static ServiceProvider ConfigureServices(RunnerSettings settings)
    {
        var services = new ServiceCollection();
        var zipBackupSettings = new ZipBackupSettings();

        if (!string.IsNullOrWhiteSpace(settings.BackupZipFileLocation))
        {
            zipBackupSettings.FileLocation = settings.BackupZipFileLocation;
        }

        services.AddSingleton(settings);
        services.AddScoped<BackupJob>();
        services.AddZipBackups(zipBackupSettings);
        services.AddLogging(loggingBuilder =>
                            {
                                loggingBuilder.ClearProviders();
                                loggingBuilder.AddConsole();
                                loggingBuilder.AddFilter(logLevel => logLevel >= settings.LogLevel);
                            });

        return services.BuildServiceProvider();
    }

    private static RunnerSettings? ParseSettings(IEnumerable<string> args)
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