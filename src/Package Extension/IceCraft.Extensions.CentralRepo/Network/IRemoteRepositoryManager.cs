// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Extensions.CentralRepo.Network;

public interface IRemoteRepositoryManager
{
    /// <summary>
    /// Gets the directory where the local repository cache is stored.
    /// </summary>
    string LocalCachedRepoPath { get; }

    Task InitializeCacheAsync();
    FileStream GetAssetFileStream(string assetName);

    /// <summary>
    /// Deletes all cached repository metadata.
    /// </summary>
    void CleanCached();
}
