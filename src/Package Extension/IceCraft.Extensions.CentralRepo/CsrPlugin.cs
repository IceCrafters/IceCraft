// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Extensions.CentralRepo;

using IceCraft.Api.Installation;
using IceCraft.Api.Plugin;
using IceCraft.Extensions.CentralRepo.Impl;
using IceCraft.Extensions.CentralRepo.Network;
using IceCraft.Extensions.CentralRepo.Runtime;
using IceCraft.Extensions.CentralRepo.Util;
using Microsoft.Extensions.DependencyInjection;

public class CsrPlugin : IPlugin
{
    public PluginMetadata Metadata { get; } = new()
    {
        Identifier = "csr",
        Version = "1.0.0",
        Name = "Central Source Repository plugin"
    };

    public void Initialize(IServiceCollection services)
    {
        services.AddRepositorySource<RemoteRepositorySourceFactory>()
            .AddSingleton<IRemoteRepositoryManager, RemoteRepositoryManager>()
            .AddSingleton<RemoteRepositoryIndexer>()
            .AddSingleton<MashiroStatePool>()
            .AddSingleton<MashiroRuntime>()
            .AddKeyedSingleton<IPackageInstaller, MashiroInstaller>("mashiro")
            .AddKeyedSingleton<IArtefactPreprocessor, MashiroPreprocessor>("mashiro")
            .AddKeyedSingleton<IPackageConfigurator, MashiroConfigurator>("mashiro")
            .AddSingleton<RepoConfigFactory>();
    }
}