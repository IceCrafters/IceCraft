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
    private readonly FrontendAppHelper _frontendApp;
    private static readonly PackageMeta MockMeta = new()
    {
        Id = "test",
        Version = new SemVersion(1, 0, 0),
        ReleaseDate = DateTime.MinValue,
        PluginInfo = new PackagePluginInfo("mashiro", "mashiro")
    };

    public ArtefactManagerTests(ITestOutputHelper outputHelper)
    {
        _frontendApp = new FrontendAppHelper(outputHelper);
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
}
