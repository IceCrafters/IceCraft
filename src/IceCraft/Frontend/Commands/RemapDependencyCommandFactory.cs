namespace IceCraft.Frontend.Commands;

using System.CommandLine;
using IceCraft.Core.Caching;
using IceCraft.Core.Installation.Analysis;
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