// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Plugin;

using System.CommandLine;
using IceCraft.Api.Plugin;
using IceCraft.Api.Plugin.Commands;

public class ExtensibleClientImpl : IExtensibleClient
{
    private readonly RootCommand _rootCommand;

    public ExtensibleClientImpl(RootCommand root)
    {
        _rootCommand = root;
    }

    public IPluginCommand CreateCommand(IPlugin plugin, string name)
    {
        var command = new Command(name, $"provided by: {plugin.Metadata.Name}");
        _rootCommand.AddCommand(command);
        return new PluginCommandImpl(command);
    }
}
