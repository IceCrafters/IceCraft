// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Tests;

using IceCraft.Api.Installation.Dependency;
using IceCraft.Api.Package;
using Semver;

public class PackageTests
{
    [Fact]
    public void ReferenceDoesPointTo_Valid_ReturnTrue()
    {
        // Arrange
        const string packageName = "test";
        var version = new SemVersion(1, 0, 0);

        var metadata = new PackageMeta(packageName, version, DateTime.UtcNow, new PackagePluginInfo("test", "test"));
        var reference = new PackageReference(packageName, version);

        // Act
        var result = reference.DoesPointTo(metadata);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CreateReference_ReturnValidReference()
    {
        // Arrange
        const string packageName = "test";
        var version = new SemVersion(1, 0, 0);

        var metadata = new PackageMeta(packageName, version, DateTime.UtcNow, new PackagePluginInfo("test", "test"));
        var reference = metadata.CreateReference();

        // Act
        var result = reference.DoesPointTo(metadata);

        // Assert
        Assert.True(result);
    }
}
