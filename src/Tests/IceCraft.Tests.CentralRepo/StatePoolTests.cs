// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Tests.CentralRepo;

using IceCraft.Api.Package;
using IceCraft.Api.Package.Data;
using IceCraft.Extensions.CentralRepo.Impl;
using IceCraft.Extensions.CentralRepo.Network;
using IceCraft.Extensions.CentralRepo.Runtime;
using IceCraft.Extensions.CentralRepo.Util;
using Semver;

public class StatePoolTests
{
    [Fact]
    public void GetScriptFileName_NoCustomData()
    {
        // Arrange
        const string fileName = "test-1.0.0.js";
        #region Data objects

        var meta = new PackageMeta("test",
            new SemVersion(1, 0, 0),
            DateTime.MinValue,
            new PackagePluginInfo("test", "test"));
        #endregion
        
        // Act
        var result = MashiroStatePool.GetScriptFileName(meta);
        
        // Assert
        Assert.Equal(fileName, result);
    }
    
    [Fact]
    public void GetScriptFileName_WithCustomData()
    {
        // Arrange
        const string fileName = "file.js";
        #region Data objects
        var data = new PackageCustomDataDictionary();
        data.AddSerialize(RemoteRepositoryIndexer.RemoteRepoData,
            new RemotePackageData(fileName),
            CsrJsonContext.Default.RemotePackageData);

        var meta = new PackageMeta("test",
            new SemVersion(1, 0, 0),
            DateTime.MinValue,
            new PackagePluginInfo("test", "test"))
        {
            CustomData = data
        };
        #endregion
        
        // Act
        var result = MashiroStatePool.GetScriptFileName(meta);
        
        // Assert
        Assert.Equal(fileName, result);
    }
}