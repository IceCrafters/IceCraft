// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Frontend.Commands;

using System.CommandLine;
using IceCraft.Api.Archive.Indexing;
using IceCraft.Api.Archive.Repositories;
using IceCraft.Api.Installation.Dependency;
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
    private readonly IServiceProvider _serviceProvider;

    public FixBrokenCommandFactory(IDependencyMapper dependencyMapper,
        IPackageIndexer indexer,
        IDependencyResolver resolver,
        IRepositorySourceManager sourceManager,
        IServiceProvider serviceProvider)
    {
        _dependencyMapper = dependencyMapper;
        _indexer = indexer;
        _resolver = resolver;
        _sourceManager = sourceManager;
        _serviceProvider = serviceProvider;
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
            try
            {
                await _resolver.ResolveTree(package, index, packages);
            }
            catch (DependencyException ex)
            {
                AnsiConsole.MarkupLine("[bold white]!!![/] [red]Unsatisified requirements[/]");
                AnsiConsole.WriteLine("IceCraft: {0}", ex.Message);
                return ExitCodes.GenericError;
            }
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

        var scope = _serviceProvider.CreateScope();
        var installer = scope.ServiceProvider.GetRequiredService<InteractiveInstaller>();
        return await installer.InstallAsync(packages, index, false);
    }
}