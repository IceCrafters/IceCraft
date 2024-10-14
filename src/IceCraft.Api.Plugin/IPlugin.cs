// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Api.Plugin;

using Microsoft.Extensions.DependencyInjection;

public interface IPlugin
{
    PluginMetadata Metadata { get; }

    [Obsolete("Service registry API is deprecated. Clients may ignore this method.")]
    void Initialize(IServiceRegistry serviceRegistry)
    {
    }

    void Initialize(IServiceCollection services);
}