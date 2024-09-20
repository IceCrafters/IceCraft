// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Extensions.CentralRepo.Impl;

using IceCraft.Api.Archive.Repositories;
using IceCraft.Extensions.CentralRepo.Network;
using Microsoft.Extensions.DependencyInjection;

public class RemoteRepositorySourceFactory : IRepositorySourceFactory
{
    public IRepositorySource CreateRepositorySource(IServiceProvider provider, out string name)
    {
        name = "csr";
        return new RemoteRepositorySource(provider.GetRequiredService<RemoteRepositoryManager>(),
                provider.GetRequiredService<RemoteRepositoryIndexer>());
    }
}