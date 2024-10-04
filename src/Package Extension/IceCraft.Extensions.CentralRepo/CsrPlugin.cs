// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Extensions.CentralRepo;

using IceCraft.Api.Installation;
using IceCraft.Api.Plugin;
using IceCraft.Extensions.CentralRepo.Impl;
using IceCraft.Extensions.CentralRepo.Network;
using IceCraft.Extensions.CentralRepo.Runtime;

public class CsrPlugin : IPlugin
{
    public PluginMetadata Metadata { get; } = new()
    {
        Identifier = "csr",
        Version = "1.0.0",
        Name = "Central Source Repository plugin"
    };
    
    public void Initialize(IServiceRegistry serviceRegistry)
    {
        serviceRegistry.RegisterRepositorySource<RemoteRepositorySourceFactory>()
            .RegisterSingleton<IRemoteRepositoryManager, RemoteRepositoryManager>()
            .RegisterSingleton<RemoteRepositoryIndexer>()
            .RegisterSingleton<MashiroStatePool>()
            .RegisterSingleton<MashiroRuntime>()
            .RegisterKeyedSingleton<IPackageInstaller, MashiroInstaller>("mashiro")
            .RegisterKeyedSingleton<IArtefactPreprocessor, MashiroPreprocessor>("mashiro")
            .RegisterKeyedSingleton<IPackageConfigurator, MashiroConfigurator>("mashiro");
    }
}