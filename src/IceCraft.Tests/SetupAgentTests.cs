// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Tests;

using System.IO.Abstractions.TestingHelpers;
using IceCraft.Api.Installation;
using IceCraft.Api.Installation.Database;
using IceCraft.Api.Package;
using IceCraft.Core.Installation;
using IceCraft.Tests.Helpers;
using Moq;
using Xunit.Abstractions;

public class SetupAgentTests
{
    private readonly FrontendAppHelper _frontendHelper;

    public SetupAgentTests(ITestOutputHelper outputHelper)
    {
        _frontendHelper = new FrontendAppHelper(outputHelper, "/ictest");
    }

    [Fact]
    public async Task Uninstall_UnconfigureThenUninstall()
    {
        // Arrange
        #region Uninstall_UnconfigureThenUninstall Arrange
        var fileSystem = new MockFileSystem();
        const string TestPkgPath = "/testPkg";

        fileSystem.AddDirectory(TestPkgPath);

        var sequence = new MockSequence();

        var meta = new PackageMeta()
        {
            Id = "test",
            PluginInfo = new PackagePluginInfo("test", "test"),
            ReleaseDate = DateTime.UtcNow,
            Version = new Semver.SemVersion(1, 0, 0)
        };

        // Setup lifetime
        var installer = new Mock<IPackageInstaller>(MockBehavior.Strict);
        var configurator = new Mock<IPackageConfigurator>(MockBehavior.Strict);
        var lifetime = new Mock<IPackageSetupLifetime>();

        lifetime.Setup(x => x.GetInstaller("test"))
            .Returns(installer.Object);
        lifetime.Setup(x => x.GetConfigurator("test"))
            .Returns(configurator.Object);

        configurator.InSequence(sequence)
            .Setup(x => x.UnconfigurePackageAsync(TestPkgPath, meta))
            .Returns(Task.CompletedTask)
            .Verifiable();
        installer.InSequence(sequence)
            .Setup(x => x.RemovePackageAsync(TestPkgPath, meta))
            .Returns(Task.CompletedTask)
            .Verifiable();
            
        // Setup install manager
        var manager = new Mock<IPackageInstallManager>();
        manager.Setup(x => x.IsInstalled(meta)).Returns(true);
        manager.Setup(x => x.GetUnsafePackageDirectory(meta)).Returns(TestPkgPath);

        var agent = new PackageSetupAgent(manager.Object,
            fileSystem,
            _frontendHelper,
            Mock.Of<ILocalDatabaseMutator>(),
            lifetime.Object);
        #endregion

        // Act
        await agent.UninstallAsync(meta);

        // Assert
        configurator.Verify();
        installer.Verify();
    }

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
