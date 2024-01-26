using CommandLine;
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

        services.AddSingleton(settings);
        services.AddScoped<BackupJob>();

        switch (settings.BackupType)
        {
            case BackupType.Zip:
                services.AddZipBackups();
                break;
            case BackupType.TarZLib:
                services.AddTarZLibBackups();
                break;
            default:
                throw new NotSupportedException("Backup type not supported.");
        }

        services.AddLogging(loggingBuilder => loggingBuilder.ClearProviders()
                                                            .AddConsole()
                                                            .AddFilter(logLevel => logLevel >= settings.LogLevel));

        return services.BuildServiceProvider();
    }

    private static RunnerSettings? ParseSettings(IEnumerable<string> args)
    {
        using var parser = new Parser();

        var parsedArgumentsResult = parser.ParseArguments<RunnerSettings>(args);

        if (!parsedArgumentsResult.Errors.Any())
        {
            return parsedArgumentsResult.Value;
        }

        parsedArgumentsResult.Errors.Output();

        return null;
    }
}