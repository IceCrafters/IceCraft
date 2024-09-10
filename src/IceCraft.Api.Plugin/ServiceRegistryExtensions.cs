namespace IceCraft.Api.Plugin;

using IceCraft.Api.Archive.Repositories;
using IceCraft.Api.Installation;
using IceCraft.Api.Package;

public static class ServiceRegistryExtensions
{
    public static void RegisterRepositorySource<T>(this IServiceRegistry registry)
        where T : class, IRepositorySourceFactory
    {
        registry.RegisterKeyedSingleton<IRepositorySourceFactory, T>(null);
    }

    /// <summary>
    /// Registers a package installer.
    /// </summary>
    /// <param name="registry">The registry to register to.</param>
    /// <param name="key">The key which will be used to refer to the registered installer by <see cref="PackagePluginInfo.InstallerRef"/>.</param>
    /// <typeparam name="T">The <see cref="IPackageInstaller"/> implementation to register.</typeparam>
    public static void RegisterInstaller<T>(this IServiceRegistry registry, string key)
        where T: class, IPackageInstaller
    {
        registry.RegisterKeyedSingleton<IPackageInstaller, T>(key);
    }
    
    /// <summary>
    /// Registers a package configurator.
    /// </summary>
    /// <param name="registry">The registry to register to.</param>
    /// <param name="key">The key which will be used to refer to the registered configurator by <see cref="PackagePluginInfo.ConfiguratorRef"/>.</param>
    /// <typeparam name="T">The <see cref="IPackageConfigurator"/> implementation to register.</typeparam>
    public static void RegisterConfigurator<T>(this IServiceRegistry registry, string key)
        where T: class, IPackageConfigurator
    {
        registry.RegisterKeyedSingleton<IPackageConfigurator, T>(key);
    }

    /// <summary>
    /// Registers an artefact preprocessor.
    /// </summary>
    /// <param name="registry">The registry to register to.</param>
    /// <param name="key">The key which will be used to refer to the registered configurator by <see cref="PackagePluginInfo.PreProcessorRef"/>.</param>
    /// <typeparam name="T">The <see cref="IArtefactPreprocessor"/> implementation to register.</typeparam>
    public static void RegisterPreprocessor<T>(this IServiceRegistry registry, string key)
        where T: class, IArtefactPreprocessor
    {
        registry.RegisterKeyedSingleton<IArtefactPreprocessor, T>(key);
    }
}