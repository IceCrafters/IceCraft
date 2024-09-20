// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Tests;

using IceCraft.Api.Installation;
using IceCraft.Api.Package;
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

    // [Fact]
    // public async Task LinkExecutableAsync_Creation()
    // {
    //     // Arrange
    //     var app = new Mock<IFrontendApp>();

    //     app.Setup(x => x.DataBasePath).Returns("/icMock");

    //     var mockFs = new MockFileSystem(new Dictionary<string, MockFileData>()
    //     {
    //         { "/icMock/runInfo.json", "{}" },
    //         { "/icMock/packages/test/test", new MockFileData([]) },
    //     });
    //     mockFs.AddDirectory("/icMock/");
    //     mockFs.AddDirectory("/icMock/run/");

    //     var exm = new ExecutableManager(app.Object, 
    //         mockFs, 
    //         GetInstallMock(), 
    //         Mock.Of<IExecutionScriptGenerator>());

    //     // Act
    //     await exm.LinkExecutableAsync(MockMeta, "test", "test");

    //     // Assert
    //     Assert.True(mockFs.FileExists("/icMock/run/test"));
    // }

    // [Fact]
    // public async Task LinkExecutableAsync_Deletion()
    // {
    //     // Arrange
    //     var app = new Mock<IFrontendApp>();

    //     app.Setup(x => x.DataBasePath).Returns("/icMock");

    //     var mockFs = new MockFileSystem(new Dictionary<string, MockFileData>()
    //     {
    //         { "/icMock/runInfo.json", "{\"test\":{\"LinkName\":\"test\",\"LinkTarget\":\"/icMock/packages/test/test\",\"PackageRef\":\"test\"}}" },
    //         { "/icMock/packages/test/test", new MockFileData([]) }
    //     });
    //     mockFs.AddDirectory("/icMock/packages/run");
    //     mockFs.File.CreateSymbolicLink("/icMock/packages/run/test", "/icMock/packages/test/test");

    //     var exm = new ExecutableManager(app.Object, mockFs, GetInstallMock(), Mock.Of<IExecutionScriptGenerator>());

    //     // Act
    //     await exm.UnlinkExecutableAsync("test");

    //     // Assert
    //     Assert.False(mockFs.FileExists("/icMock/run/test"));
    // }
}