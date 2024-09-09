namespace IceCraft.Frontend.Cli;

using System;
using System.CommandLine;
using IceCraft.Core.Archive.Indexing;
using IceCraft.Core.Archive.Repositories;
using IceCraft.Core.Caching;
using IceCraft.Core.Configuration;
using IceCraft.Core.Installation;
using IceCraft.Core.Installation.Analysis;
using IceCraft.Core.Network;
using IceCraft.Core.Platform;
using IceCraft.Frontend.Commands;
using Microsoft.Extensions.DependencyInjection;

internal static class RootCommandFactory
{
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

        var downloadCmd = new DownloadCommandFactory(sourceManager,
                indexer,
            serviceProvider.GetRequiredService<IDownloadManager>(),
            serviceProvider.GetRequiredService<IMirrorSearcher>())
            .CreateCommand();
        
        var infoCmd = new InfoCommandFactory(indexer, sourceManager)
            .CreateCommand();

        var initCmd = new InitializeCommandFactory(serviceProvider.GetRequiredService<IEnvironmentManager>(),
            serviceProvider.GetRequiredService<IFrontendApp>())
            .CreateCommand();

        var cacheCmd = new CacheCommandFactory(serviceProvider.GetRequiredService<ICacheManager>())
            .CreateCommand();

        var packageCmd = CreatePackageCmd(sourceManager, indexer, 
            serviceProvider.GetRequiredService<IMirrorSearcher>(),
            serviceProvider);

        var installCmd = new InstallCommandFactory(serviceProvider)
            .CreateCommand();

        var removeCmd = new CliRemoveCommandFactory(serviceProvider.GetRequiredService<IPackageInstallManager>(),
            serviceProvider.GetRequiredService<IDependencyMapper>())
            .CreateCommand();

        // Create root command

        var root = new RootCommand("Command line interactive frontend for IceCraft")
        {
            updateCmd,
            sourceCmd,
            downloadCmd,
            infoCmd,
            initCmd,
            cacheCmd,
            packageCmd,
            installCmd,
            removeCmd
        };

        // Configure verbose options
        root.AddGlobalOption(FrontendUtil.OptVerbose);
        root.AddGlobalOption(FrontendUtil.OptDebug);

        return root;
    }

    private static Command CreatePackageCmd(IRepositorySourceManager sourceManager,
        IPackageIndexer indexer,
        IMirrorSearcher mirrorSearcher,
        IServiceProvider serviceProvider)
    {
        // Assemble commands

        var bestMirror = new BestMirrorCommandFactory(indexer,
            sourceManager,
            mirrorSearcher)
            .CreateCli();

        var fixBroken = new FixBrokenCommandFactory(serviceProvider)
            .CreateCommand();

        var list = new ListVersionCommandFactory(sourceManager, indexer, serviceProvider.GetRequiredService<IFrontendApp>())
            .CreateCommand();

        var reconfigure = new ReconfigureCommandFactory(serviceProvider,
            serviceProvider.GetRequiredService<IPackageInstallManager>())
            .CreateCommand();

        var regenDependMap =
            new RemapDependencyCommandFactory(serviceProvider.GetRequiredService<IDependencyMapper>())
                .CreateCommand();

        return new Command("package", "Perform various package tasks")
        {
            bestMirror,
            fixBroken,
            list,
            reconfigure,
            regenDependMap
        };
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
