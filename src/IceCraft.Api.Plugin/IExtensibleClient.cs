// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Api.Plugin;

using IceCraft.Api.Plugin.Commands;

/// <summary>
/// Defines an interface that allows a client to declare various extensibility
/// APIs.
/// </summary>
public interface IExtensibleClient
{
    IPluginCommand CreateCommand(IPlugin plugin, string name);
}
