// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Extensions.CentralRepo;

using System;
using IceCraft.Api.Installation;
using IceCraft.Api.Plugin;
using IceCraft.Extensions.CentralRepo.Api;
using IceCraft.Extensions.CentralRepo.Client;
using IceCraft.Extensions.CentralRepo.Impl;
using IceCraft.Extensions.CentralRepo.Network;
using IceCraft.Extensions.CentralRepo.Runtime;
using IceCraft.Extensions.CentralRepo.Runtime.Security;
using IceCraft.Extensions.CentralRepo.Util;
using Jint;
using Microsoft.Extensions.DependencyInjection;

public class CsrPlugin : IPlugin, IClientExtension
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
            .AddInstaller<MashiroInstaller>("mashiro")
            .AddPreprocessor<MashiroPreprocessor>("mashiro")
            .AddConfigurator<MashiroConfigurator>("mashiro")
            .AddSingleton<RepoConfigFactory>()
            .AddScoped<ContextApiRoot>()
            .AddScoped<IMashiroApiProvider, MashiroApiProvider>()
            .AddScoped<MashiroState>()
            .AddScoped<IMashiroMetaTransfer, MashiroMetaTransfer>()
            .AddTransient<Func<ILocalPackageImporter>>(ctx => ctx.GetRequiredService<ILocalPackageImporter>)
            .AddTransient(_ => MashiroRuntime.CreateJintEngine())
            .AddTransient<IMashiroLifetimeFactory, MashiroLifetimeFactory>()
            .AddSingleton<CsrBuildCommand>();
        
        AddMashiroApis(services);
    }

    private static void AddMashiroApis(IServiceCollection services)
    {
        services.AddScoped<IMashiroBinaryApi, MashiroBinary>()
            .AddScoped<IMashiroAssetsApi, MashiroAssets>()
            .AddScoped<IMashiroCompressedArchiveApi, MashiroCompressedArchive>()
            .AddScoped<IMashiroConsoleApi, MashiroConsole>()
            .AddScoped<IMashiroFsApi, MashiroFs>()
            .AddScoped<IMashiroOsApi, MashiroOs>()
            .AddScoped<IMashiroPackagesApi, MashiroPackages>();
    }

    public void InitializeClient(IExtensibleClient client, IServiceProvider serviceProvider)
    {
        var command = client.CreateCommand(this, "build");
        var buildCmd = serviceProvider.GetRequiredService<CsrBuildCommand>();
        buildCmd.Configure(command);
    }
}