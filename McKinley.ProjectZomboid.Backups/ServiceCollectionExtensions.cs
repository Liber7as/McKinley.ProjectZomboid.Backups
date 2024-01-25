using System.IO.Abstractions;
using McKinley.ProjectZomboid.Backups.Abstractions;
using McKinley.ProjectZomboid.Backups.Settings;
using McKinley.ProjectZomboid.Backups.TarZLib;
using McKinley.ProjectZomboid.Backups.Zip;
using Microsoft.Extensions.DependencyInjection;

namespace McKinley.ProjectZomboid.Backups;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddZipBackups(this IServiceCollection services, BackupSettings? settings = null)
    {
        RegisterDefaultServicesAndSettings(services, settings);

        services.AddScoped<IBackupService, ZipBackupService>();

        return services;
    }

    public static IServiceCollection AddTarZLibBackups(this IServiceCollection services, BackupSettings? settings = null)
    {
        RegisterDefaultServicesAndSettings(services, settings);

        services.AddScoped<IBackupService, TarZLibBackupService>();

        return services;
    }

    private static void RegisterDefaultServicesAndSettings(IServiceCollection services, BackupSettings? settings = null)
    {
        services.AddSingleton(settings ?? new BackupSettings());

        services.AddScoped<IFileSystem, FileSystem>();
        services.AddScoped<ISaveService, SaveService>();
        services.AddScoped<IZipBackupService, ZipBackupService>();
        services.AddScoped<ITarZLibBackupService, TarZLibBackupService>();
    }
}