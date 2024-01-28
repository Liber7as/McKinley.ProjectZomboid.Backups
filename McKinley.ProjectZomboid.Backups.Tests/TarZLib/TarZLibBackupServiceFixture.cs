using System.IO.Abstractions;
using McKinley.ProjectZomboid.Backups.Abstractions;
using McKinley.ProjectZomboid.Backups.TarZLib;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace McKinley.ProjectZomboid.Backups.Tests.TarZLib;

/// <summary>
/// Tests TarZLib backups for Project Zomboid saves. In order for these tests to work, you must place a Project Zomboid save in
/// the "Saves" folder.
/// </summary>
[TestFixture]
public class TarZLibBackupServiceFixture
{
    [SetUp]
    public void Setup()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTarZLibBackups();
        serviceCollection.AddTestLogging();

        var serviceProvider = serviceCollection.BuildServiceProvider();
        _fileSystem = serviceProvider.GetRequiredService<IFileSystem>();
        _saveService = (SaveService) serviceProvider.GetRequiredService<ISaveService>();
        _backupService = (TarZLibBackupService) serviceProvider.GetRequiredService<IBackupService>();
    }

    private TarZLibBackupService _backupService = null!;
    private SaveService _saveService = null!;
    private IFileSystem _fileSystem = null!;

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

    [Test]
    public async Task RestoreAsync()
    {
        await using var memoryStream = new MemoryStream();

        foreach (var save in await _saveService.GetAsync(TestHelper.SaveDirectory))
        {
            await _backupService.BackupAsync(save, memoryStream);
        }

        var backupDirectory = _fileSystem.DirectoryInfo.New("Restored-Backups");

        if (backupDirectory.Exists)
        {
            backupDirectory.Delete(true);
        }

        memoryStream.Seek(0, SeekOrigin.Begin);

        await _backupService.RestoreAsync(memoryStream, backupDirectory);
    }
}