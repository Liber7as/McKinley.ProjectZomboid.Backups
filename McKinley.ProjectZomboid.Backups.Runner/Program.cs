using CommandLine;
using McKinley.ProjectZomboid.Backups.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace McKinley.ProjectZomboid.Backups.Runner;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        var parsedArgs = ParseArguments(args);

        if (parsedArgs == null)
        {
            return -1;
        }

        await using var serviceProvider = ConfigureServices(parsedArgs);

        var backupJob = serviceProvider.GetRequiredService<BackupJob>();

        return await backupJob.RunAsync(parsedArgs);
    }

    private static ServiceProvider ConfigureServices(CommandLineArgumentsModel args)
    {
        var services = new ServiceCollection();

        var compressionSettings = new CompressionSettings
        {
            CompressionLevel = args.CompressionLevel
        };

        services.AddSingleton(args);
        services.AddScoped<BackupJob>();

        switch (args.BackupType)
        {
            case BackupType.Zip:
                services.AddZipBackups(compressionSettings);
                break;
            case BackupType.TarZLib:
                services.AddTarZLibBackups(compressionSettings);
                break;
            default:
                throw new NotSupportedException("Backup type not supported.");
        }

        services.AddLogging(loggingBuilder => loggingBuilder.ClearProviders()
                                                            .AddConsole()
                                                            .AddFilter(logLevel => logLevel >= args.LogLevel));

        return services.BuildServiceProvider();
    }

    private static CommandLineArgumentsModel? ParseArguments(IEnumerable<string> args)
    {
        using var parser = new Parser(settings => settings.HelpWriter = Console.Out);

        var parsedArgumentsResult = parser.ParseArguments<CommandLineArgumentsModel>(args);

        if (!parsedArgumentsResult.Errors.Any())
        {
            return parsedArgumentsResult.Value;
        }

        parsedArgumentsResult.Errors.Output();

        return null;
    }
}