// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Tests;

using IceCraft.Api.Archive.Indexing;
using IceCraft.Tests.Helpers;
using Semver;

public class VersioningTests
{
    [Fact]
    public void GetLatestSemVersion_PackageDict_ReturnLatest()
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
    public void GetLatestSemVersionOrDefault_PackageDict_ReturnLatest()
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