using System.IO.Abstractions;
using McKinley.ProjectZomboid.Backups.Abstractions;
using McKinley.ProjectZomboid.Backups.Zip.Services;
using McKinley.ProjectZomboid.Backups.Zip.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace McKinley.ProjectZomboid.Backups;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddZipBackups(this IServiceCollection services, ZipBackupSettings? settings = null)
    {
        services.AddSingleton(settings ?? new ZipBackupSettings());

        services.AddScoped<IFileSystem, FileSystem>();
        services.AddScoped<ISaveService, SaveService>();
        services.AddScoped<IBackupService, ZipBackupService>();

        return services;
    }
}