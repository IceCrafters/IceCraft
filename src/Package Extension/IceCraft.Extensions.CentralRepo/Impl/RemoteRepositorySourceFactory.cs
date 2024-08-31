namespace IceCraft.Extensions.CentralRepo.Impl;

using IceCraft.Core.Archive.Providers;
using IceCraft.Core.Caching;
using IceCraft.Core.Platform;
using Microsoft.Extensions.DependencyInjection;

public class RemoteRepositorySourceFactory : IRepositorySourceFactory
{
    public IRepositorySource CreateRepositorySource(IServiceProvider provider, out string name)
    {
        name = "csr";
        return new RemoteRepositorySource(provider.GetRequiredService<IFrontendApp>(),
                provider.GetRequiredService<ICacheManager>());
    }
}