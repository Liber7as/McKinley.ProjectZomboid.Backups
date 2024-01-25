using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using McKinley.ProjectZomboid.Backups.Abstractions;
using McKinley.ProjectZomboid.Backups.Abstractions.Models;
using Microsoft.Extensions.Logging;

namespace McKinley.ProjectZomboid.Backups;

public class SaveService : ISaveService
{
    /// <summary>
    /// The file to check for within a directory to determine if it is a Project Zomboid save.
    /// </summary>
    private const string SaveIdentifierFileName = "map.bin";

    private readonly ILogger<SaveService>? _logger;

    public SaveService(ILogger<SaveService>? logger = null)
    {
        _logger = logger;
    }

    public Task<IEnumerable<Save>> GetAsync(IDirectoryInfo saveDirectory)
    {
        _logger?.LogInformation($"Reading saves from '{saveDirectory.FullName}'");

        var saves = saveDirectory.EnumerateFiles(SaveIdentifierFileName, SearchOption.AllDirectories)
                                 .Select(file => file.Directory)
                                 .Where(directory => directory != null)
                                 .Select(directory => new Save(directory!))
                                 .ToArray();

        _logger?.LogInformation($"Found {saves.Length} saves");

        foreach (var save in saves)
        {
            _logger?.LogDebug($"Save: '{save.Name}'");
        }

        return Task.FromResult<IEnumerable<Save>>(saves);
    }
}