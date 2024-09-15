namespace IceCraft.Extensions.CentralRepo;

using IceCraft.Api.Archive.Repositories;
using IceCraft.Extensions.CentralRepo.Impl;
using IceCraft.Extensions.CentralRepo.Network;
using IceCraft.Extensions.CentralRepo.Runtime;
using Microsoft.Extensions.DependencyInjection;

public static class CsrDependencyExtensions
{
    public static IServiceCollection AddCsrExtension(this IServiceCollection services)
    {
        return services.AddKeyedSingleton<IRepositorySourceFactory, RemoteRepositorySourceFactory>(null)
            .AddSingleton<RemoteRepositoryManager>()
            .AddSingleton<MashiroStatePool>();
    }
}