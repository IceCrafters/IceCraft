namespace IceCraft.Extensions.CentralRepo.Impl;

using IceCraft.Api.Archive.Repositories;
using IceCraft.Api.Caching;
using IceCraft.Api.Client;
using IceCraft.Core.Caching;
using IceCraft.Core.Platform;
using IceCraft.Extensions.CentralRepo.Network;
using Microsoft.Extensions.DependencyInjection;

public class RemoteRepositorySourceFactory : IRepositorySourceFactory
{
    public IRepositorySource CreateRepositorySource(IServiceProvider provider, out string name)
    {
        name = "csr";
        return new RemoteRepositorySource(provider.GetRequiredService<IFrontendApp>(),
                provider.GetRequiredService<ICacheManager>(),
                provider.GetRequiredService<RemoteRepositoryManager>());
    }
}