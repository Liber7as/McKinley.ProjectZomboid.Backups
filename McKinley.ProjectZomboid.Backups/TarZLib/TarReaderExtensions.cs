using System.Collections.Generic;
using System.Formats.Tar;

namespace McKinley.ProjectZomboid.Backups.TarZLib;

internal static class TarReaderExtensions
{
    internal static async IAsyncEnumerable<TarEntry> GetEntriesAsync(this TarReader tarReader)
    {
        TarEntry? entry;
        while ((entry = await tarReader.GetNextEntryAsync(true)) != null)
        {
            yield return entry;
        }
    }
}