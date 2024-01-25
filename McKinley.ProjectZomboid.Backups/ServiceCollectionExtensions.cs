using System.IO.Abstractions;
using McKinley.ProjectZomboid.Backups.Abstractions;
using McKinley.ProjectZomboid.Backups.Settings;
using McKinley.ProjectZomboid.Backups.Zip.Services;
using Microsoft.Extensions.DependencyInjection;

namespace McKinley.ProjectZomboid.Backups;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddZipBackups(this IServiceCollection services, BackupSettings? settings = null)
    {
        services.AddSingleton(settings ?? new BackupSettings());

        services.AddScoped<IFileSystem, FileSystem>();
        services.AddScoped<ISaveService, SaveService>();
        services.AddScoped<IBackupService, ZipBackupService>();

        return services;
    }
}