namespace IceCraft.Frontend;

using System;
using System.CommandLine;
using IceCraft.Core.Archive.Indexing;
using IceCraft.Core.Archive.Repositories;
using IceCraft.Core.Caching;
using IceCraft.Core.Configuration;
using IceCraft.Core.Network;
using IceCraft.Core.Platform;
using IceCraft.Frontend.Commands;
using Microsoft.Extensions.DependencyInjection;

internal static class CliFrontend
{
    internal static readonly Option<bool> OptVerbose = new("--verbose", "Enable verbose output");
    private static readonly Option<bool> OptDebug = new("--debug", "Allow debugger attach confirmation before acting");

    public static RootCommand CreateCommand(IServiceProvider serviceProvider)
    {
        // Commonly used dependencies
        
        var indexer = serviceProvider.GetRequiredService<IPackageIndexer>();
        var sourceManager = serviceProvider.GetRequiredService<IRepositorySourceManager>();

        // Assemble commands
        
        var updateCmd = new UpdateCommand(sourceManager,
                indexer)
                .CreateCli();

        var sourceCmd = CreateSourceCmd(serviceProvider);

        var downloadCmd = new CliDownloadCommand(sourceManager,
                indexer,
            serviceProvider.GetRequiredService<IDownloadManager>(),
            serviceProvider.GetRequiredService<IMirrorSearcher>())
            .CreateCli();
        
        var infoCmd = new CliInfoCommand(indexer, sourceManager)
            .CreateCli();

        var initCmd = new InitializeCommand(serviceProvider.GetRequiredService<IEnvironmentManager>(),
            serviceProvider.GetRequiredService<IFrontendApp>())
            .CreateCli();

        var cacheCmd = new CliCacheCommand(serviceProvider.GetRequiredService<ICacheManager>())
            .CreateCli();

        // Create root command

        var root = new RootCommand("Command line interactive frontend for IceCraft")
        {
            updateCmd,
            sourceCmd,
            downloadCmd,
            infoCmd,
            initCmd,
            cacheCmd
        };

        // Configure verbose options
        root.AddGlobalOption(OptVerbose);
        root.AddGlobalOption(OptDebug);

        return root;
    }

    private static Command CreateSourceCmd(IServiceProvider serviceProvider)
    {
        var command = new Command("source", "Enable, disable or modify sources");

        var sourceManager = serviceProvider.GetRequiredService<IRepositorySourceManager>();
        var configuration = serviceProvider.GetRequiredService<IManagerConfiguration>();

        command.AddCommand(new SourceSwitchCommand.EnableCommand(sourceManager, configuration)
            .CreateCli("enable"));
        command.AddCommand(new SourceSwitchCommand.DisableCommand(sourceManager, configuration)
            .CreateCli("disable"));

        return command;
    }
}
