// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Frontend.Commands;

using System.CommandLine;
using IceCraft.Api.Archive.Artefacts;
using IceCraft.Api.Archive.Indexing;
using IceCraft.Api.Archive.Repositories;
using IceCraft.Api.Installation;
using IceCraft.Api.Installation.Dependency;
using IceCraft.Api.Network;
using IceCraft.Frontend.Cli;
using IceCraft.Interactive;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

public class FixBrokenCommandFactory : ICommandFactory
{
    private readonly IDependencyMapper _dependencyMapper;
    private readonly IPackageIndexer _indexer;
    private readonly IDependencyResolver _resolver;
    private readonly IRepositorySourceManager _sourceManager;
    private readonly InteractiveInstaller _installer;

    public FixBrokenCommandFactory(IServiceProvider serviceProvider)
    {
        _dependencyMapper = serviceProvider.GetRequiredService<IDependencyMapper>();
        _indexer = serviceProvider.GetRequiredService<IPackageIndexer>();
        _resolver = serviceProvider.GetRequiredService<IDependencyResolver>();
        _sourceManager = serviceProvider.GetRequiredService<IRepositorySourceManager>();

        _installer = new InteractiveInstaller(serviceProvider.GetRequiredService<IDownloadManager>(),
            serviceProvider.GetRequiredService<IPackageInstallManager>(),
            serviceProvider.GetRequiredService<IArtefactManager>(),
            serviceProvider.GetRequiredService<IChecksumRunner>(),
            serviceProvider.GetRequiredService<IDependencyMapper>());
    }

    public Command CreateCommand()
    {
        var optDryRun = new Option<bool>("--dry-run");

        var command = new Command("fix-broken", "Install missing dependencies")
        {
            optDryRun
        };
        
        command.SetHandler(async context => context.ExitCode = 
            await ExecuteAsync(context.GetOpt(optDryRun)));

        return command;
    }
    
    private async Task<int> ExecuteAsync(bool dryRun)
    {
        var packages = new HashSet<DependencyLeaf>();
        var index = await _indexer.IndexAsync(_sourceManager);

        await foreach (var package in _dependencyMapper.EnumerateUnsatisifiedPackages())
        {
            await _resolver.ResolveTree(package, index, packages);
        }

        if (packages.Count == 0)
        {
            Output.Shared.Log("Dependencies are OK, nothing to install");
            return ExitCodes.Ok;
        }

        if (dryRun)
        {
            AnsiConsole.Write(new Columns(packages.Select(p => p.Package.Id)));
            return ExitCodes.Ok;
        }

        if (!InteractiveInstaller.AskConfirmation(packages))
        {
            return ExitCodes.Ok;
        }

        return await _installer.InstallAsync(packages, index, false);
    }
}