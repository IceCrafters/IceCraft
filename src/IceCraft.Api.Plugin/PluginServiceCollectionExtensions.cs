// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Api.Plugin;

using IceCraft.Api.Archive.Repositories;
using IceCraft.Api.Installation;
using IceCraft.Api.Installation.Dependency;
using Microsoft.Extensions.DependencyInjection;

public static class PluginServiceCollectionExtensions
{
    public static IServiceCollection AddRepositorySource<T>(this IServiceCollection registry)
        where T : class, IRepositorySourceFactory
    {
        return registry.AddSingleton<IRepositorySourceFactory, T>();
    }

    /// <summary>
    /// Adds a package installer.
    /// </summary>
    /// <param name="registry">The registry to register to.</param>
    /// <param name="key">The key which will be used to refer to the registered installer by <see cref="PackagePluginInfo.InstallerRef"/>.</param>
    /// <typeparam name="T">The <see cref="IPackageInstaller"/> implementation to register.</typeparam>
    public static IServiceCollection AddInstaller<T>(this IServiceCollection registry, string key)
        where T : class, IPackageInstaller
    {
        return registry.AddKeyedScoped<IPackageInstaller, T>(key);
    }

    /// <summary>
    /// Adds a package configurator.
    /// </summary>
    /// <param name="registry">The registry to register to.</param>
    /// <param name="key">The key which will be used to refer to the registered configurator by <see cref="PackagePluginInfo.ConfiguratorRef"/>.</param>
    /// <typeparam name="T">The <see cref="IPackageConfigurator"/> implementation to register.</typeparam>
    public static IServiceCollection AddConfigurator<T>(this IServiceCollection registry, string key)
        where T : class, IPackageConfigurator
    {
        return registry.AddKeyedScoped<IPackageConfigurator, T>(key);
    }

    /// <summary>
    /// Adds an artefact preprocessor.
    /// </summary>
    /// <param name="registry">The registry to register to.</param>
    /// <param name="key">The key which will be used to refer to the registered configurator by <see cref="PackagePluginInfo.PreProcessorRef"/>.</param>
    /// <typeparam name="T">The <see cref="IArtefactPreprocessor"/> implementation to register.</typeparam>
    public static IServiceCollection AddPreprocessor<T>(this IServiceCollection registry, string key)
        where T : class, IArtefactPreprocessor
    {
        return registry.AddKeyedScoped<IArtefactPreprocessor, T>(key);
    }

    public static IServiceCollection AddDependencyHook<T>(this IServiceCollection registry)
        where T : class, IDependencyHook
    {
        return registry.AddSingleton<IDependencyHook>();
    }
}
