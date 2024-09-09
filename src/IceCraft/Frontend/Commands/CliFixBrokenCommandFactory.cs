namespace IceCraft.Frontend.Commands;

using System.CommandLine;
using IceCraft.Core.Archive.Artefacts;
using IceCraft.Core.Archive.Checksums;
using IceCraft.Core.Archive.Dependency;
using IceCraft.Core.Archive.Indexing;
using IceCraft.Core.Archive.Packaging;
using IceCraft.Core.Archive.Repositories;
using IceCraft.Core.Installation;
using IceCraft.Core.Installation.Analysis;
using IceCraft.Core.Network;
using IceCraft.Frontend.Cli;
using IceCraft.Interactive;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

public class CliFixBrokenCommandFactory
{
    private readonly IDependencyMapper _dependencyMapper;
    private readonly IPackageIndexer _indexer;
    private readonly IDependencyResolver _resolver;
    private readonly IRepositorySourceManager _sourceManager;
    private readonly InteractiveInstaller _installer;

    public CliFixBrokenCommandFactory(IServiceProvider serviceProvider)
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

    public Command CreateCli()
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
        var packages = new HashSet<PackageMeta>();
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
            AnsiConsole.Write(new Columns(packages.Select(p => p.Id)));
            return ExitCodes.Ok;
        }

        if (!_installer.AskConfirmation(packages))
        {
            return ExitCodes.Ok;
        }

        return await _installer.InstallAsync(packages, index);
    }
}