// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Tests;

using IceCraft.Api.Installation;
using IceCraft.Api.Package;
using IceCraft.Core.Installation;
using Moq;

public class PackageImporterTests
{
    [Fact]
    public void LocalPackageImporter_CallImport()
    {
        // Arrange
        var meta = new PackageMeta()
        {
            Id = "test",
            PluginInfo = new PackagePluginInfo("test", "test"),
            ReleaseDate = DateTime.UtcNow,
            Version = new Semver.SemVersion(1, 0, 0)
        };
        var configurator = new Mock<IPackageConfigurator>();

        var lifetime = new Mock<IPackageSetupLifetime>();
        lifetime.Setup(x => x.GetConfigurator("test")).Returns(configurator.Object);

        var installManager = new Mock<IPackageInstallManager>();
        installManager.Setup(x => x.IsInstalled(meta)).Returns(true);
        installManager.Setup(x => x.GetInstalledPackageDirectory(meta)).Returns("/test-pkg");

        var importer = new LocalPackageImporter(installManager.Object, lifetime.Object);

        // Act
        importer.ImportEnvironment(meta);

        // Assert
        configurator.Verify(x => x.ExportEnvironment("/test-pkg", meta));
    }
}
