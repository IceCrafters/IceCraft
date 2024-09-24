// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Plugin;

using IceCraft.Api.Plugin;

public class PluginManager
{
    private readonly List<IPlugin> _plugins = [];

    public void Add(IPlugin plugin)
    {
        _plugins.Add(plugin);
    }

    public void InitializeAll(IServiceRegistry serviceRegistry)
    {
        foreach (var plugin in _plugins)
        {
            plugin.Initialize(serviceRegistry);
        }
    }
}