// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Extensions.CentralRepo.Impl;

using System.Collections.Generic;
using IceCraft.Api.Archive.Repositories;
using IceCraft.Extensions.CentralRepo.Network;

public class RemoteRepositorySource : AsyncRepositorySource
{
    private readonly IRemoteRepositoryManager _remoteManager;
    private readonly RemoteRepositoryIndexer _repositoryIndexer;

    public RemoteRepositorySource(IRemoteRepositoryManager remoteManager, 
        RemoteRepositoryIndexer repositoryIndexer)
    {
        _remoteManager = remoteManager;
        _repositoryIndexer = repositoryIndexer;
    }

    public override async IAsyncEnumerable<RepositoryInfo> CreateRepositoriesAsync()
    {
        await _remoteManager.InitializeCacheAsync();
        var (count, series) = await _repositoryIndexer.IndexSeries();

        yield return new RepositoryInfo("csr", new RemoteRepository(series, count));
    }

    public async Task<IRepository?> CreateRepositoryAsync()
    {
        await _remoteManager.InitializeCacheAsync();
        var (count, series) = await _repositoryIndexer.IndexSeries();

        return new RemoteRepository(series, count);
    }

    public override Task RefreshAsync()
    {
        _remoteManager.CleanCached();
        return Task.CompletedTask;
    }
}