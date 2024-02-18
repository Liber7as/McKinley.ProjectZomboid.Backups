using System.IO.Abstractions;
using McKinley.ProjectZomboid.Backups.Abstractions;
using McKinley.ProjectZomboid.Backups.TarBrotli;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace McKinley.ProjectZomboid.Backups.Tests.TarBrotli;

/// <summary>
/// Tests TarBrotli backups for Project Zomboid saves. In order for these tests to work, you must place a Project Zomboid
/// save in the "Saves" folder.
/// </summary>
[TestFixture]
public class TarBrotliBackupServiceFixture
{
    [SetUp]
    public void Setup()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTarBrotliBackups();
        serviceCollection.AddTestLogging();

        var serviceProvider = serviceCollection.BuildServiceProvider();
        _saveService = (SaveService) serviceProvider.GetRequiredService<ISaveService>();
        _backupService = (TarBrotliBackupService) serviceProvider.GetRequiredService<IBackupService>();
    }

    private TarBrotliBackupService _backupService = null!;
    private SaveService _saveService = null!;

    [Test]
    public async Task BackupAsync()
    {
        await using var memoryStream = new MemoryStream();

        foreach (var save in await _saveService.GetAsync(TestHelper.SaveDirectory))
        {
            await _backupService.BackupAsync(save, memoryStream);
        }

        // TODO: Ensure everything is created
    }
}