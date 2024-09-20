// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Extensions.CentralRepo.Impl;

using IceCraft.Api.Archive.Repositories;
using IceCraft.Extensions.CentralRepo.Network;

public class RemoteRepositorySource : IRepositorySource
{
    private readonly RemoteRepositoryManager _remoteManager;
    private readonly RemoteRepositoryIndexer _repositoryIndexer;

    public RemoteRepositorySource(RemoteRepositoryManager remoteManager, 
        RemoteRepositoryIndexer repositoryIndexer)
    {
        _remoteManager = remoteManager;
        _repositoryIndexer = repositoryIndexer;
    }
    
    public async Task<IRepository?> CreateRepositoryAsync()
    {
        await _remoteManager.InitializeCacheAsync();
        var (count, series) = await _repositoryIndexer.IndexSeries();

        return new RemoteRepository(series, count);
    }

    public Task RefreshAsync()
    {
        _remoteManager.CleanPrevious();
        return Task.CompletedTask;
    }
}