// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Frontend.Commands;

using System.CommandLine;
using System.Threading.Tasks;
using IceCraft.Api.Archive.Indexing;
using IceCraft.Api.Archive.Repositories;
using IceCraft.Api.Client;
using IceCraft.Frontend.Cli;
using Spectre.Console;

public sealed class ListVersionCommandFactory : ICommandFactory
{
    private readonly IRepositorySourceManager _sourceManager;
    private readonly IPackageIndexer _indexer;
    private readonly IFrontendApp _frontend;

    public ListVersionCommandFactory(IRepositorySourceManager sourceManager,
        IPackageIndexer indexer,
        IFrontendApp frontend)
    {
        _sourceManager = sourceManager;
        _indexer = indexer;
        _frontend = frontend;
    }

    public Command CreateCommand()
    {
        var argPackage = new Argument<string>("package", "Package to list");
        var optPrerelease = new Option<bool>(["-P", "--include-prerelease"], "Include prerelease versions in output");

        var command = new Command("list", "List package versions")
        {
            argPackage,
            optPrerelease
        };

        command.SetHandler(async context => context.ExitCode = await ExecuteInternalAsync(
            context.GetArgNotNull(argPackage),
            context.GetOpt(optPrerelease),
            context.GetCancellationToken()
        ));

        return command;
    }
    
    private async Task<int> ExecuteInternalAsync(string packageName, bool includePrerelease, CancellationToken cancellationToken)
    {
        var index = await _indexer.IndexAsync(_sourceManager, _frontend.GetCancellationToken());

        if (!index.TryGetValue(packageName, out var seriesInfo))
        {
            _frontend.Output.Error("package not found: {0}", packageName);
            return ExitCodes.PackageNotFound;
        }

        foreach (var version in seriesInfo.Versions
                     .Where(x => !x.Value.Metadata.Version.IsPrerelease || includePrerelease))
        {
            cancellationToken.ThrowIfCancellationRequested();
            AnsiConsole.WriteLine(version.Key);
        }

        return ExitCodes.Ok;
    }
}
