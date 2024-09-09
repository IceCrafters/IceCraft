namespace IceCraft.Frontend;

using System.CommandLine;
using IceCraft.Core.Archive.Indexing;
using IceCraft.Core.Archive.Repositories;
using IceCraft.Core.Caching;

#if LEGACY_INTERFACE
using System.ComponentModel;
using JetBrains.Annotations;
using Spectre.Console.Cli;
#endif

using CliCommand = System.CommandLine.Command;

#if LEGACY_INTERFACE
[Description("Regenerate all package refs")]
[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
#endif

public class UpdateCommand
#if LEGACY_INTERFACE
    : AsyncCommand<UpdateCommand.Settings>
#endif
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

#if LEGACY_INTERFACE
    public sealed class Settings : BaseSettings
    {
        [CommandOption("--index-only")]
        [Description("Only reindex the packages but do not refresh the sources.")]
        public bool IndexOnly { get; init; }
    }
#endif

    private readonly IRepositorySourceManager _sourceManager;
    private readonly IPackageIndexer _indexer;

    public UpdateCommand(IRepositorySourceManager sourceManager,
        IPackageIndexer indexer)
    {
        _sourceManager = sourceManager;
        _indexer = indexer;
    }

#if LEGACY_INTERFACE
    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        return await ExecuteAsync(settings.IndexOnly);
    }
#endif
}