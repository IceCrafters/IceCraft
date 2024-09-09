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

        var downloadCmd = new CliDownloadCommandFactory(sourceManager,
                indexer,
            serviceProvider.GetRequiredService<IDownloadManager>(),
            serviceProvider.GetRequiredService<IMirrorSearcher>())
            .CreateCli();
        
        var infoCmd = new CliInfoCommandFactory(indexer, sourceManager)
            .CreateCli();

        var initCmd = new InitializeCommand(serviceProvider.GetRequiredService<IEnvironmentManager>(),
            serviceProvider.GetRequiredService<IFrontendApp>())
            .CreateCli();

        var cacheCmd = new CliCacheCommandFactory(serviceProvider.GetRequiredService<ICacheManager>())
            .CreateCli();

        var packageCmd = CreatePackageCmd(sourceManager, indexer, 
            serviceProvider.GetRequiredService<IMirrorSearcher>(),
            serviceProvider);

        var installCmd = new InstallCommand(serviceProvider)
            .CreateCli();

        var removeCmd = new CliRemoveCommandFactory(serviceProvider.GetRequiredService<IPackageInstallManager>(),
            serviceProvider.GetRequiredService<IDependencyMapper>())
            .CreateCli();

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

        var bestMirror = new CliBestMirrorCommandFactory(indexer,
            sourceManager,
            mirrorSearcher)
            .CreateCli();

        var fixBroken = new CliFixBrokenCommandFactory(serviceProvider)
            .CreateCli();

        var list = new PackageListCommand(sourceManager, indexer, serviceProvider.GetRequiredService<IFrontendApp>())
            .CreateCli();

        var reconfigure = new CliReconfigureCommandFactory(serviceProvider,
            serviceProvider.GetRequiredService<IPackageInstallManager>())
            .CreateCli();

        var regenDependMap =
            new CliRemapDependencyCommandFactory(serviceProvider.GetRequiredService<IDependencyMapper>())
                .CreateCli();

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
