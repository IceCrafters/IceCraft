namespace IceCraft.Core;

using IceCraft.Core.Archive.Checksums;
using IceCraft.Core.Archive.Indexing;
using IceCraft.Core.Archive.Repositories;
using IceCraft.Core.Installation;
using IceCraft.Core.Installation.Execution;
using IceCraft.Core.Installation.Storage;
using IceCraft.Core.Network;
using Microsoft.Extensions.DependencyInjection;

public static class IceCraftDependencyExtensions
{
    public static IServiceCollection AddIceCraftDefaults(this IServiceCollection services)
    {
        return services.AddIceCraftHashers()
            .AddIceCraftServices();
    }

    public static IServiceCollection AddIceCraftServices(this IServiceCollection services)
    {
        return services.AddSingleton<IMirrorSearcher, MirrorSearcher>()
            .AddSingleton<IDownloadManager, DownloadManager>()
            .AddSingleton<IRepositorySourceManager, RepositoryManager>()
            .AddSingleton<IChecksumRunner, DependencyChecksumRunner>()
            .AddSingleton<IPackageIndexer, CachedIndexer>()
            .AddSingleton<IPackageInstallDatabaseFactory, PackageInstallDatabaseFactory>()
            .AddSingleton<IPackageInstallManager, PackageInstallManager>()
            .AddSingleton<IExecutableManager, ExecutableManager>();
    }

    public static IServiceCollection AddIceCraftHashers(this IServiceCollection services)
    {
        return services.AddKeyedSingleton<IChecksumValidator, Sha256ChecksumValidator>("sha256");
    }
}
