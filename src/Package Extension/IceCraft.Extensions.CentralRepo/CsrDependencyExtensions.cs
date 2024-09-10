namespace IceCraft.Extensions.CentralRepo;

using IceCraft.Api.Archive.Repositories;
using IceCraft.Extensions.CentralRepo.Impl;
using Microsoft.Extensions.DependencyInjection;

public static class CsrDependencyExtensions
{
    public static IServiceCollection AddCsrExtension(this IServiceCollection services)
    {
        return services.AddKeyedSingleton<IRepositorySourceFactory, RemoteRepositorySourceFactory>(null);
    }
}