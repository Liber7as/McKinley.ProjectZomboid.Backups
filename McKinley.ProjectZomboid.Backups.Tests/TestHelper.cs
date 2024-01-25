using System.IO.Abstractions;
using NUnit.Framework;

namespace McKinley.ProjectZomboid.Backups.Tests;

public class TestHelper
{
    public static IDirectoryInfo SaveDirectory => new FileSystem().DirectoryInfo.New(Path.Combine(TestContext.CurrentContext.WorkDirectory, "../../../Saves/"));
}