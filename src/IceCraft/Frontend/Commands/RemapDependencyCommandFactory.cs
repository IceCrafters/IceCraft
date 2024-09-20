// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Frontend.Commands;

using System.CommandLine;
using IceCraft.Api.Caching;
using IceCraft.Api.Installation.Dependency;
using IceCraft.Frontend.Cli;

public class RemapDependencyCommandFactory : ICommandFactory
{
    private readonly IDependencyMapper _dependencyMapper;

    public RemapDependencyCommandFactory(IDependencyMapper dependencyMapper)
    {
        _dependencyMapper = dependencyMapper;
    }
    
    public Command CreateCommand()
    {
        var command = new Command("regen-dependmap", "Regenerate the dependency map cache");
        command.SetHandler(Execute);
        return command;
    }

    private async Task Execute()
    {
        if (_dependencyMapper is ICacheClearable clearable)
        {
            clearable.ClearCache();
        }

        await _dependencyMapper.MapDependenciesCached();
    }
}