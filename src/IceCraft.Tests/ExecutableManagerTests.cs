namespace IceCraft.Tests;

using System.IO.Abstractions.TestingHelpers;
using IceCraft.Core.Archive.Packaging;
using IceCraft.Core.Installation.Execution;
using IceCraft.Core.Platform;
using Moq;

public class ExecutableManagerTests
{
    [Fact]
    public async Task LinkExecutableAsync_Creation()
    {
        // Arrange
        var app = new Mock<IFrontendApp>();

        app.Setup(x => x.DataBasePath).Returns("/home/test/icecraft");

        var mockFs = new MockFileSystem(new Dictionary<string, MockFileData>()
        {
            { "/home/test/icecraft/runInfo.json", "{}" },
            { "/home/test/icecraft/packages/test/test", new MockFileData([]) },
        });

        var exm = new ExecutableManager(app.Object, mockFs);
        var packageMeta = new PackageMeta()
        {
            Id = "test",
            PluginInfo = new("test", "test"),
            ReleaseDate = DateTime.MinValue,
            Version = "0.0.0"
        };

        // Act
        await exm.LinkExecutableAsync(packageMeta, "test", "/home/test/icecraft/packages/test/test");

        // Assert
        Assert.True(mockFs.FileExists("/home/test/icecraft/run/test"));
    }

    [Fact]
    public async Task LinkExecutableAsync_Deletion()
    {
        // Arrange
        var app = new Mock<IFrontendApp>();

        app.Setup(x => x.DataBasePath).Returns("/home/test/icecraft");

        var mockFs = new MockFileSystem(new Dictionary<string, MockFileData>()
        {
            { "/home/test/icecraft/runInfo.json", "{\"test\":{\"LinkName\":\"test\",\"LinkTarget\":\"/home/test/icecraft/packages/test/test\",\"PackageRef\":\"test\"}}" },
            { "/home/test/icecraft/packages/test/test", new MockFileData([]) }
        });
        mockFs.AddDirectory("/home/test/icecraft/packages/run");
        mockFs.File.CreateSymbolicLink("/home/test/icecraft/packages/run/test", "/home/test/icecraft/packages/test/test");

        var exm = new ExecutableManager(app.Object, mockFs);
        var packageMeta = new PackageMeta()
        {
            Id = "test",
            PluginInfo = new("test", "test"),
            ReleaseDate = DateTime.MinValue,
            Version = "0.0.0"
        };

        // Act
        await exm.UnlinkExecutableAsync("test");

        // Assert
        Assert.False(mockFs.FileExists("/home/test/icecraft/run/test"));
    }
}