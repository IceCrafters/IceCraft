using System;
using IceCraft.Core.Caching;
using Serilog;
using Spectre.Console.Cli;

namespace IceCraft.Frontend.Commands;

public class ClearCacheCommand : Command<BaseSettings>
{
    private readonly ICacheManager _cacheManager;

    public ClearCacheCommand(ICacheManager cacheManager)
    {
        _cacheManager = cacheManager;
    }

    public override int Execute(CommandContext context, BaseSettings settings)
    {
        Log.Verbose("Removing cache");

        _cacheManager.RemoveAll();
        return 0;
    }
}
