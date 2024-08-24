namespace IceCraft.Core;

using IceCraft.Core.Archive.Artefacts;
using IceCraft.Core.Archive.Checksums;
using IceCraft.Core.Archive.Dependency;
using IceCraft.Core.Archive.Indexing;
using IceCraft.Core.Archive.Repositories;
using IceCraft.Core.Installation;
using IceCraft.Core.Installation.Analysis;
using IceCraft.Core.Installation.Execution;
using IceCraft.Core.Installation.Storage;
using IceCraft.Core.Network;
using IceCraft.Core.Platform;
using IceCraft.Core.Platform.Linux;
using IceCraft.Core.Platform.Windows;
using Microsoft.Extensions.DependencyInjection;

public static class IceCraftDependencyExtensions
{
    public static IServiceCollection AddIceCraftDefaults(this IServiceCollection services)
    {
        return services.AddChecksumValidators()
            .AddIceCraftServices()
            .AddIceCraftPlatform();
    }

    private static IServiceCollection AddIceCraftPlatform(this IServiceCollection services)
    {
        if (OperatingSystem.IsLinux())
        {
            return services.AddSingleton<IEnvironmentManager, LinuxEnvironmentManager>()
                .AddSingleton<IExecutionScriptGenerator, PosixExecutionScriptGenerator>();
        }

        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (OperatingSystem.IsWindows())
        {
            return services.AddSingleton<IEnvironmentManager, WindowsEnvironmentManager>();
        }

        return services;
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
            .AddSingleton<IExecutableManager, ExecutableManager>()
            .AddSingleton<IDependencyResolver, DependencyResolver>()
            .AddSingleton<IDependencyMapper, DependencyMapper>()
            .AddKeyedSingleton<IPackageInstaller, VirtualInstaller>("virtual")
            .AddKeyedSingleton<IPackageConfigurator, VirtualConfigurator>("virtual")
            .AddSingleton<IArtefactManager, ArtefactManager>();
    }

    public static IServiceCollection AddChecksumValidators(this IServiceCollection services)
    {
        return services.AddKeyedSingleton<IChecksumValidator, Sha256ChecksumValidator>("sha256")
            .AddKeyedSingleton<IChecksumValidator, Sha512ChecksumValidator>("sha512");
    }
}
