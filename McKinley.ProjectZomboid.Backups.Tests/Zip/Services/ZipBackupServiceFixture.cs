﻿using McKinley.ProjectZomboid.Backups.Abstractions;
using McKinley.ProjectZomboid.Backups.Abstractions.Models;
using McKinley.ProjectZomboid.Backups.Zip.Services;
using McKinley.ProjectZomboid.Backups.Zip.Settings;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace McKinley.ProjectZomboid.Backups.Tests.Zip.Services;

[TestFixture]
public class ZipBackupServiceFixture
{
    [SetUp]
    public void Setup()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddZipBackups();

        var serviceProvider = serviceCollection.BuildServiceProvider();
        _settings = serviceProvider.GetRequiredService<ZipBackupSettings>();
        _saveService = (SaveService) serviceProvider.GetRequiredService<ISaveService>();
        _backupService = (ZipBackupService) serviceProvider.GetRequiredService<IBackupService>();
    }

    private ZipBackupService _backupService = null!;
    private SaveService _saveService = null!;
    private ZipBackupSettings _settings = null!;

    [Test]
    public async Task BackupAsync()
    {
        // Ensure the backup file is deleted
        var backupFileInfo = new FileInfo(_settings.FileLocation);

        if (backupFileInfo.Exists)
        {
            backupFileInfo.Delete();
        }

        foreach (var save in await _saveService.GetAsync(TestHelper.SaveDirectory))
        {
            await _backupService.BackupAsync(save);
        }

        // TODO: Ensure everything is created
    }

    [Test]
    public async Task BackupMultipleAsync()
    {
        // Ensure the backup file is deleted
        var backupFileInfo = new FileInfo(_settings.FileLocation);

        if (backupFileInfo.Exists)
        {
            backupFileInfo.Delete();
        }

        // Backup twice:

        foreach (var save in await _saveService.GetAsync(TestHelper.SaveDirectory))
        {
            await _backupService.BackupAsync(save);
        }

        foreach (var save in await _saveService.GetAsync(TestHelper.SaveDirectory))
        {
            await _backupService.BackupAsync(save);
        }

        // TODO: Ensure everything is created
    }
}