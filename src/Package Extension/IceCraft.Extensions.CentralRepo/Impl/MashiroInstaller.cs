// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Extensions.CentralRepo.Impl;

using IceCraft.Api.Installation;
using IceCraft.Api.Package;
using IceCraft.Extensions.CentralRepo.Runtime;
using IceCraft.Extensions.CentralRepo.Runtime.Security;

public class MashiroInstaller : IPackageInstaller
{
    private readonly MashiroStatePool _statePool;

    public MashiroInstaller(MashiroStatePool statePool)
    {
        _statePool = statePool;
    }
    
    public async Task ExpandPackageAsync(string artefactFile, string targetDir, PackageMeta package)
    {
        var state = await _statePool.GetAsync(package);
        state.EnsureMetadata();
        if (state.ExpandPackageDelegate == null)
        {
            throw new InvalidOperationException($"No installer registered for package {package.Id} ({package.Version})");
        }
        
        state.DoContext(ExecutionContextType.Installation,
            () => state.ExpandPackageDelegate(artefactFile, targetDir));
    }

    public async Task RemovePackageAsync(string targetDir, PackageMeta package)
    {
        var state = await _statePool.GetAsync(package);
        state.EnsureMetadata();
        if (state.RemovePackageDelegate == null)
        {
            throw new InvalidOperationException($"No installer registered for package {package.Id} ({package.Version})");
        }
        
        state.DoContext(ExecutionContextType.Installation,
            () => state.RemovePackageDelegate(targetDir));
    }
}