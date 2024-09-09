namespace IceCraft.Frontend.Commands;
using IceCraft.Core.Caching;
using Spectre.Console.Cli;

public class ClearCacheCommand : Command<BaseSettings>
{
    private readonly ICacheManager _cacheManager;

    public ClearCacheCommand(ICacheManager cacheManager)
    {
        _cacheManager = cacheManager;
    }

    public override int Execute(CommandContext context, BaseSettings settings)
    {
        Output.Shared.Verbose("Removing cache...");

        _cacheManager.RemoveAll();
        return 0;
    }
}
