namespace IceCraft.Tests;

using System.IO.Abstractions.TestingHelpers;
using IceCraft.Core.Archive.Packaging;
using IceCraft.Core.Installation;
using IceCraft.Core.Installation.Execution;
using IceCraft.Core.Platform;
using Moq;

public class ExecutableManagerTests
{
    #region Helpers
    private static IPackageInstallManager GetInstallMock()
    {
        var mock = new Mock<IPackageInstallManager>();

        mock.Setup(x => x.GetInstalledPackageDirectoryAsync(MockMeta))
            .Returns(Task.FromResult("/icMock/packages/test/"));

        return mock.Object;
    }

    private static readonly PackageMeta MockMeta = new()
    {
        Id = "test",
        PluginInfo = new("test", "test"),
        ReleaseDate = DateTime.MinValue,
        Version = "0.0.0"
    };
    #endregion

    [Fact]
    public async Task LinkExecutableAsync_Creation()
    {
        // Arrange
        var app = new Mock<IFrontendApp>();

        app.Setup(x => x.DataBasePath).Returns("/icMock");

        var mockFs = new MockFileSystem(new Dictionary<string, MockFileData>()
        {
            { "/icMock/runInfo.json", "{}" },
            { "/icMock/packages/test/test", new MockFileData([]) },
        });

        var exm = new ExecutableManager(app.Object, mockFs, GetInstallMock());

        // Act
        await exm.LinkExecutableAsync(MockMeta, "test", "test");

        // Assert
        Assert.True(mockFs.FileExists("/icMock/run/test"));
    }

    [Fact]
    public async Task LinkExecutableAsync_Deletion()
    {
        // Arrange
        var app = new Mock<IFrontendApp>();

        app.Setup(x => x.DataBasePath).Returns("/icMock");

        var mockFs = new MockFileSystem(new Dictionary<string, MockFileData>()
        {
            { "/icMock/runInfo.json", "{\"test\":{\"LinkName\":\"test\",\"LinkTarget\":\"/icMock/packages/test/test\",\"PackageRef\":\"test\"}}" },
            { "/icMock/packages/test/test", new MockFileData([]) }
        });
        mockFs.AddDirectory("/icMock/packages/run");
        mockFs.File.CreateSymbolicLink("/icMock/packages/run/test", "/icMock/packages/test/test");

        var exm = new ExecutableManager(app.Object, mockFs, GetInstallMock());

        // Act
        await exm.UnlinkExecutableAsync("test");

        // Assert
        Assert.False(mockFs.FileExists("/icMock/run/test"));
    }
}