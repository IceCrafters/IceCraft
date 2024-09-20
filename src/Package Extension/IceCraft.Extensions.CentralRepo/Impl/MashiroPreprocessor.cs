// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Extensions.CentralRepo.Impl;

using IceCraft.Api.Installation;
using IceCraft.Api.Package;
using IceCraft.Extensions.CentralRepo.Runtime;
using IceCraft.Extensions.CentralRepo.Runtime.Security;

public class MashiroPreprocessor : IArtefactPreprocessor
{
    private readonly MashiroStatePool _statePool;

    public MashiroPreprocessor(MashiroStatePool statePool)
    {
        _statePool = statePool;
    }
    
    public async Task Preprocess(string tempExpandDir, string installDir, PackageMeta meta)
    {
        var state = await _statePool.GetAsync(meta);
        state.EnsureMetadata();
        if (state.PreprocessPackageDelegate == null)
        {
            throw new InvalidOperationException($"No preprocessor registered for package {meta.Id} ({meta.Version})");
        }
        
        state.DoContext(ExecutionContextType.Installation,
            () => state.PreprocessPackageDelegate(tempExpandDir, installDir));
    }
}