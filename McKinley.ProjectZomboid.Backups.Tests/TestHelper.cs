using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace McKinley.ProjectZomboid.Backups.Tests;

public static class TestHelper
{
    public static IDirectoryInfo SaveDirectory => new FileSystem().DirectoryInfo.New(Path.Combine(TestContext.CurrentContext.WorkDirectory, "../../../Saves/"));

    public static void AddTestLogging(this IServiceCollection services)
    {
        services.AddLogging(loggingBuilder => loggingBuilder.ClearProviders()
                                                            .AddDebug()
                                                            .AddFilter(_ => true));
    }
}