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

        if (BackupFileInfo.Exists)
        {
            BackupFileInfo.Delete();
        }
    }

    private TarZLibBackupService _backupService = null!;
    private SaveService _saveService = null!;
    private IFileSystem _fileSystem = null!;

    private IFileInfo BackupFileInfo => _fileSystem.FileInfo.New("ProjectZomboid-Backups.tar.zl");

    [Test]
    public async Task BackupAsync()
    {
        foreach (var save in await _saveService.GetAsync(TestHelper.SaveDirectory))
        {
            await _backupService.BackupAsync(save, BackupFileInfo);
        }

        // TODO: Ensure everything is created
    }
}