// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Frontend;

using System.CommandLine;
using IceCraft.Api.Archive.Indexing;
using IceCraft.Api.Archive.Repositories;
using IceCraft.Api.Caching;

using CliCommand = System.CommandLine.Command;

public class UpdateCommand
{
    private static readonly Option<bool> IndexOnly = new("--index-only",
        "Instruct package source providers to reindex package sources but do not refresh caches");

    internal CliCommand CreateCli()
    {
        var command = new CliCommand("update", "Refreshes package sources")
        {
            IndexOnly
        };

        command.SetHandler(async context =>
        {
            var indexOnly = context.ParseResult.GetValueForOption(IndexOnly);

            context.ExitCode = await ExecuteAsync(indexOnly);
        });

        return command;
    }

    private async Task<int> ExecuteAsync(bool indexOnly)
    {
        if (!indexOnly)
        {
            var sourceNum = 0;

            foreach (var source in _sourceManager.EnumerateSources())
            {
                sourceNum++;
                await source.RefreshAsync();
            }

            Output.Shared.Log("Refreshed {0} sources", sourceNum);
        }

        // ReSharper disable once InvertIf
        // Justification: ICacheClearable is a special case
        if (_indexer is ICacheClearable clearable)
        {
            Output.Shared.Log("Updating packages index");

            clearable.ClearCache();
            await _indexer.IndexAsync(_sourceManager);
        }

        return 0;
    }

    private readonly IRepositorySourceManager _sourceManager;
    private readonly IPackageIndexer _indexer;

    public UpdateCommand(IRepositorySourceManager sourceManager,
        IPackageIndexer indexer)
    {
        _sourceManager = sourceManager;
        _indexer = indexer;
    }
}