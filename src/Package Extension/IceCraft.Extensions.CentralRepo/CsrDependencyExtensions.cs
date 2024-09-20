// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Extensions.CentralRepo;

using IceCraft.Api.Archive.Repositories;
using IceCraft.Api.Installation;
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
            .AddSingleton<RemoteRepositoryIndexer>()
            .AddSingleton<MashiroStatePool>()
            .AddSingleton<MashiroRuntime>()
            .AddKeyedSingleton<IPackageInstaller, MashiroInstaller>("mashiro")
            .AddKeyedSingleton<IArtefactPreprocessor, MashiroPreprocessor>("mashiro")
            .AddKeyedSingleton<IPackageConfigurator, MashiroConfigurator>("mashiro");
    }
}
