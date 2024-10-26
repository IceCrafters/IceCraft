// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Tests;

using IceCraft.Api.Archive.Indexing;
using IceCraft.Api.Installation.Database;
using IceCraft.Tests.Helpers;
using Moq;
using Semver;

public class VersioningTests
{
    [Fact]
    public void GetLatestVersionOrDefault_OlderAndNewer_ReturnsNewer()
    {
        // Arrange
        var versionA = SemVersion.Parse("1.0.0");
        var versionB = SemVersion.Parse("2.0.0");
        
        var readHandle = new Mock<ILocalDatabaseReadHandle>();
        readHandle.Setup(x => x.EnumerateEntries(It.IsAny<string>()))
            .Returns([
                MetaHelper.GetFakeInstalledInfo(MetaHelper.CreateMeta(versionA)),
                MetaHelper.GetFakeInstalledInfo(MetaHelper.CreateMeta(versionB))
            ]);

        // Act
        var result = readHandle.Object.GetLatestVersionOrDefault("test");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(versionB, result.Version);
    }
    
    [Fact]
    public void GetLatestEntryVersionOrDefault_OlderAndNewer_ReturnsNewer()
    {
        // Arrange
        var versionA = SemVersion.Parse("1.0.0");
        var versionB = SemVersion.Parse("2.0.0");
        
        var readHandle = new Mock<ILocalDatabaseReadHandle>();
        readHandle.Setup(x => x.EnumerateEntries(It.IsAny<string>()))
            .Returns([
                MetaHelper.GetFakeInstalledInfo(MetaHelper.CreateMeta(versionA)),
                MetaHelper.GetFakeInstalledInfo(MetaHelper.CreateMeta(versionB))
            ]);

        // Act
        var result = readHandle.Object.GetLatestVersionEntryOrDefault("test");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(versionB, result.Metadata.Version);
    }

    [Fact]
    public void GetLatestSemVersion_OlderAndNewer_ReturnsNewer()
    {
        // Arrange
        const string versionA = "1.0.0";
        const string versionB = "2.0.0";
        var cachedInfoA = MetaHelper.CreateCachedInfo(SemVersion.Parse(versionA));
        var cachedInfoB = MetaHelper.CreateCachedInfo(SemVersion.Parse(versionB));

        var dictionary = new Dictionary<string, CachedPackageInfo>(2)
        {
            { versionA, cachedInfoA },
            { versionB, cachedInfoB }
        };

        // Act
        var latest = dictionary.GetLatestSemVersion();

        // Assert
        Assert.Equal(cachedInfoB.Metadata.Version, latest);
    }

    [Fact]
    public void GetLatestSemVersionOrDefault_OlderAndNewer_ReturnsNewer()
    {
        // Arrange
        const string versionA = "1.0.0";
        const string versionB = "2.0.0";
        var cachedInfoA = MetaHelper.CreateCachedInfo(SemVersion.Parse(versionA));
        var cachedInfoB = MetaHelper.CreateCachedInfo(SemVersion.Parse(versionB));

        var dictionary = new Dictionary<string, CachedPackageInfo>(2)
        {
            { versionA, cachedInfoA },
            { versionB, cachedInfoB }
        };

        // Act
        var latest = dictionary.GetLatestSemVersionOrDefault();

        // Assert
        Assert.Equal(cachedInfoB.Metadata.Version, latest);
    }
}