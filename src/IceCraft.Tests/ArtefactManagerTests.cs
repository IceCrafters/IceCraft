// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Tests;

using System.IO.Abstractions.TestingHelpers;
using IceCraft.Api.Archive.Artefacts;
using IceCraft.Api.Package;
using IceCraft.Core.Archive.Artefacts;
using IceCraft.Tests.Helpers;
using Moq;
using Semver;
using Xunit.Abstractions;

public class ArtefactManagerTests
{
    private const string MockBase = "/ic/";

    private readonly FrontendAppHelper _frontendApp;
    private static readonly PackageMeta MockMeta = new()
    {
        Id = "test",
        Version = new SemVersion(1, 0, 0),
        ReleaseDate = DateTime.MinValue,
        PluginInfo = new PackagePluginInfo("mashiro", "mashiro")
    };
    private static readonly PackageMeta MockMetaB = new()
    {
        Id = "test2",
        Version = new SemVersion(1, 0, 0),
        ReleaseDate = DateTime.MinValue,
        PluginInfo = new PackagePluginInfo("mashiro", "mashiro")
    };

    public ArtefactManagerTests(ITestOutputHelper outputHelper)
    {
        _frontendApp = new FrontendAppHelper(outputHelper, MockBase);
    }

    [Fact]
    public void CleanArtefacts_RemoveSevenDayLongFiles()
    {
        // Arrange
        var oldDate = DateTime.UtcNow - TimeSpan.FromDays(12);
        var newDate = DateTime.UtcNow - TimeSpan.FromDays(1);
        var fs = new MockFileSystem();
        fs.AddFile("/ic/artefacts/oldFile", new MockFileData("OLD")
        {
            CreationTime = oldDate
        });
        fs.AddFile("/ic/artefacts/newFile", new MockFileData("NEW")
        {
            CreationTime = newDate
        });

        var manager = new ArtefactManager(_frontendApp, Mock.Of<IChecksumRunner>(), fs);

        // Act
        manager.CleanArtefacts();

        // Assert
        Assert.False(fs.FileExists("/ic/artefacts/oldFile"));
        Assert.True(fs.FileExists("/ic/artefacts/newFile"));
    }

    [Fact]
    public void CreateArtefactFile_Hashed_CreateFileAndOpenWritableStream()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var artefactManager = new ArtefactManager(_frontendApp,
            Mock.Of<IChecksumRunner>(),
            fileSystem);
        var artefact = new HashedArtefact("test", "test");
        var path = artefactManager.GetArtefactPath(artefact, MockMeta);

        // Act
        using var stream = artefactManager.CreateArtefactFile(artefact, MockMeta);

        // Assert
        Assert.True(stream.CanWrite);
        Assert.True(fileSystem.FileExists(path));
    }

    [Fact]
    public void CreateArtefactFile_Volatile_Throw()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var artefactManager = new ArtefactManager(_frontendApp,
            Mock.Of<IChecksumRunner>(),
            fileSystem);
        var artefact = new VolatileArtefact();

        // Act
        var exception = Record.Exception(() => artefactManager.CreateArtefactFile(artefact, MockMeta));

        // Assert
        Assert.IsType<ArgumentException>(exception);
    }

    [Fact]
    public void GetArtefactPath_AlwaysNullWhenVolatile()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var artefactManager = new ArtefactManager(_frontendApp,
            Mock.Of<IChecksumRunner>(),
            fileSystem);

        // Act
        var result = artefactManager.GetArtefactPath(new VolatileArtefact(), MockMeta);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetArtefactPath_UniqueForEachPackage()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var artefactManager = new ArtefactManager(_frontendApp,
            Mock.Of<IChecksumRunner>(),
            fileSystem);
        var artefact = new HashedArtefact("test", "test");

        // Act
        var resultA = artefactManager.GetArtefactPath(artefact, MockMeta);
        var resultB = artefactManager.GetArtefactPath(artefact, MockMetaB);

        // Assert
        Assert.NotEqual(resultA, resultB);
    }

    [Fact]
    public async Task GetSafeArtefactPathAsync_FileNotExist_ReturnNull()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var artefactManager = new ArtefactManager(_frontendApp,
            Mock.Of<IChecksumRunner>(),
            fileSystem);
        var artefact = new HashedArtefact("test", "test");

        // Act
        var result = await artefactManager.GetSafeArtefactPathAsync(artefact, MockMeta);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetSafeArtefactPathAsync_FileExistAndHashValid_ReturnPath()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var checksumRunner = new Mock<IChecksumRunner>();
        var artefact = new HashedArtefact("test", "test");
        checksumRunner.Setup(x => x.ValidateAsync(artefact, It.IsAny<Stream>()))
            .Returns(Task.FromResult(true));

        var artefactManager = new ArtefactManager(_frontendApp,
            checksumRunner.Object,
            fileSystem);

        var realPath = artefactManager.GetArtefactPath(artefact, MockMeta);
        fileSystem.AddFile(realPath, "DATA");

        // Act
        var result = await artefactManager.GetSafeArtefactPathAsync(artefact, MockMeta);

        // Assert
        Assert.Equal(realPath, result);
    }

    [Fact]
    public async Task GetSafeArtefactPathAsync_FileExistButHashInvalid_ReturnNull()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var checksumRunner = new Mock<IChecksumRunner>();
        var artefact = new HashedArtefact("test", "test");
        checksumRunner.Setup(x => x.ValidateAsync(artefact, It.IsAny<Stream>()))
            .Returns(Task.FromResult(false));

        var artefactManager = new ArtefactManager(_frontendApp,
            checksumRunner.Object,
            fileSystem);

        var realPath = artefactManager.GetArtefactPath(artefact, MockMeta);
        fileSystem.AddFile(realPath, "DATA");

        // Act
        var result = await artefactManager.GetSafeArtefactPathAsync(artefact, MockMeta);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task VerifyArtefact_AlwaysFalseWhenVolatile()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var artefactManager = new ArtefactManager(_frontendApp,
            Mock.Of<IChecksumRunner>(),
            fileSystem);

        // Act
        var result = await artefactManager.VerifyArtefactAsync(new VolatileArtefact(), MockMeta);

        // Assert
        Assert.False(result);
    }
}
