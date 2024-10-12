// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Tests;

using System.IO.Abstractions.TestingHelpers;
using IceCraft.Api.Installation;
using IceCraft.Api.Package;
using IceCraft.Core.Installation;
using Moq;

public class SetupAgentTests
{
    [Fact]
    public async Task ExpandInternal_CallExpander()
    {
        const string from = "/icMock/artefact.zip";
        const string to = "/icMock/target/";

        // Arrange
        var installer = new Mock<IPackageInstaller>();
        var meta = new PackageMeta()
        {
            Id = "test",
            PluginInfo = new PackagePluginInfo("test", "test"),
            ReleaseDate = DateTime.UtcNow,
            Version = new Semver.SemVersion(1, 0, 0)
        };
        var fs = new MockFileSystem();
        fs.AddFile(from, "artefact");
        fs.AddDirectory(to);

        // Act
        await PackageSetupAgent.InternalExpandAsync(meta,
            from,
            to,
            fs,
            installer.Object,
            null);

        // Assert
        installer.Verify(x => x.ExpandPackageAsync(from, to, meta), Times.Once());
    }

    [Fact]
    public async Task ExpandInternal_CallPreprocessor()
    {
        const string from = "/icMock/artefact.zip";
        const string to = "/icMock/target/";

        // Arrange
        var preprocessor = new Mock<IArtefactPreprocessor>();
        var meta = new PackageMeta()
        {
            Id = "test",
            PluginInfo = new PackagePluginInfo("test", "test"),
            ReleaseDate = DateTime.UtcNow,
            Version = new Semver.SemVersion(1, 0, 0)
        };
        var fs = new MockFileSystem(new MockFileSystemOptions()
        {
            CreateDefaultTempDir = true
        });
        fs.AddFile(from, "artefact");
        fs.AddDirectory(to);

        // Act
        await PackageSetupAgent.InternalExpandAsync(meta,
            from,
            to,
            fs,
            Mock.Of<IPackageInstaller>(),
            preprocessor.Object);

        // Assert
        preprocessor.Verify(x => x.Preprocess(It.IsAny<string>(), to, meta), Times.Once);
    }
}
