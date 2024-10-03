// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Extensions.CentralRepo.Api;
using IceCraft.Extensions.CentralRepo.Network;
using IceCraft.Extensions.CentralRepo.Runtime.Security;

public class MashiroAssets : ContextApi
{
    private readonly RemoteRepositoryManager _repoManager;

    public MashiroAssets(ContextApiRoot parent,
        RemoteRepositoryManager repoManager) : base(ExecutionContextType.Installation, parent)
    {
        _repoManager = repoManager;
    }
    
    public MashiroAssetHandle GetAsset(string assetName)
    {
        EnsureContext();

        var stream = _repoManager.GetAssetFileStream(assetName);
        return new MashiroAssetHandle(assetName, stream);
    }
}
