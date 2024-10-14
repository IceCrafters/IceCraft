// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Plugin;
using IceCraft.Api.Configuration;
using IceCraft.Api.Plugin;
using IceCraft.Frontend.Configuration;
using IceCraft.Interactive;
using Microsoft.Extensions.DependencyInjection;

public class ClientPlugin : IPlugin
{
    public PluginMetadata Metadata { get; } = new PluginMetadata
    {
        Identifier = "icecraft-client",
        Version = IceCraftApp.ProductVersion
    };

    public void Initialize(IServiceCollection services)
    {
        services.AddSingleton<IConfigManager, ClientConfigManager>()
            .AddTransient<InteractiveInstaller>();
    }
}
