namespace IceCraft.Frontend.Commands;
using System.ComponentModel;
using IceCraft.Core.Caching;
using IceCraft.Core.Installation.Analysis;
using Spectre.Console.Cli;

[Description("Regenerates dependency map.")]
public class RegenerateDependMapCommand : AsyncCommand<BaseSettings>
{
    private readonly IDependencyMapper _dependencyMapper;

    public RegenerateDependMapCommand(IDependencyMapper dependencyMapper)
    {
        _dependencyMapper = dependencyMapper;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, BaseSettings settings)
    {
        if (_dependencyMapper is ICacheClearable clearable)
        {
            clearable.ClearCache();
        }

        await _dependencyMapper.MapDependenciesCached();
        return 0;
    }
}
