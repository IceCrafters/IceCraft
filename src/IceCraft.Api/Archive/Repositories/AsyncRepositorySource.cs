// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Api.Archive.Repositories;

public abstract class AsyncRepositorySource : IAsyncRepositorySource, IRepositorySource
{
    public virtual IEnumerable<RepositoryInfo> CreateRepositories()
    {
        return CreateRepositoriesAsync().ToBlockingEnumerable();
    }

    public abstract IAsyncEnumerable<RepositoryInfo> CreateRepositoriesAsync();

    public abstract Task RefreshAsync();
}
