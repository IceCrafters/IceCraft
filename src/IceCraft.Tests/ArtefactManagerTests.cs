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
