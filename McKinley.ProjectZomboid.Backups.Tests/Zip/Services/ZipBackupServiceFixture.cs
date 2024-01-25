using System.IO.Abstractions;
using McKinley.ProjectZomboid.Backups.Abstractions;
using McKinley.ProjectZomboid.Backups.Settings;
using McKinley.ProjectZomboid.Backups.Zip.Services;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace McKinley.ProjectZomboid.Backups.Tests.Zip.Services;

/// <summary>
/// Tests zip backups for Project Zomboid saves. In order for these tests to work, you must place a Project Zomboid save in
/// the "Saves" folder.
/// </summary>
[TestFixture]
public class ZipBackupServiceFixture
{
    [SetUp]
    public void Setup()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddZipBackups();

        var serviceProvider = serviceCollection.BuildServiceProvider();
        _fileSystem = serviceProvider.GetRequiredService<IFileSystem>();
        _settings = serviceProvider.GetRequiredService<BackupSettings>();
        _saveService = (SaveService) serviceProvider.GetRequiredService<ISaveService>();
        _backupService = (ZipBackupService) serviceProvider.GetRequiredService<IBackupService>();
    }

    private ZipBackupService _backupService = null!;
    private SaveService _saveService = null!;
    private BackupSettings _settings = null!;
    private IFileSystem _fileSystem = null!;

    [Test]
    public async Task BackupAsync()
    {
        // Ensure the backup file is deleted
        var backupFileInfo = _fileSystem.FileInfo.New("ProjectZomboid-Backups.zip");

        if (backupFileInfo.Exists)
        {
            backupFileInfo.Delete();
        }

        foreach (var save in await _saveService.GetAsync(TestHelper.SaveDirectory))
        {
            await _backupService.BackupAsync(save, backupFileInfo);
        }

        // TODO: Ensure everything is created
    }

    [Test]
    public async Task BackupMultipleAsync()
    {
        // Ensure the backup file is deleted
        var backupFileInfo = _fileSystem.FileInfo.New("ProjectZomboid-Backups.zip");

        if (backupFileInfo.Exists)
        {
            backupFileInfo.Delete();
        }

        // Backup twice:

        foreach (var save in await _saveService.GetAsync(TestHelper.SaveDirectory))
        {
            await _backupService.BackupAsync(save, backupFileInfo);
        }

        foreach (var save in await _saveService.GetAsync(TestHelper.SaveDirectory))
        {
            await _backupService.BackupAsync(save, backupFileInfo);
        }

        // TODO: Ensure everything is created
    }
}