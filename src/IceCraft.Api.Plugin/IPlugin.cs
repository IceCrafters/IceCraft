// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Api.Plugin;

public interface IPlugin
{
    PluginMetadata Metadata { get; }

    void Initialize(IServiceRegistry serviceRegistry);
}