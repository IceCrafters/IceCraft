namespace IceCraft.Extensions.CentralRepo;

using IceCraft.Core.Archive.Providers;
using IceCraft.Extensions.CentralRepo.Impl;
using Microsoft.Extensions.DependencyInjection;

public static class CsrDependencyExtensions
{
    public static IServiceCollection AddCsrExtension(this IServiceCollection services)
    {
        return services.AddKeyedSingleton<IRepositorySourceFactory, RemoteRepositorySourceFactory>(null);
    }
}