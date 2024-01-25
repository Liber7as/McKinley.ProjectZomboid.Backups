using NUnit.Framework;

namespace McKinley.ProjectZomboid.Backups.Tests;

public class TestHelper
{
    public static DirectoryInfo SaveDirectory => new(Path.Combine(TestContext.CurrentContext.WorkDirectory, "../../../Saves/"));
}