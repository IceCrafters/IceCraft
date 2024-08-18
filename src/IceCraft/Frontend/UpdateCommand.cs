namespace IceCraft.Frontend;

using System.ComponentModel;
using IceCraft.Core.Archive.Indexing;
using IceCraft.Core.Archive.Repositories;
using IceCraft.Core.Caching;
using JetBrains.Annotations;
using Serilog;
using Spectre.Console.Cli;

[Description("Regenerate all package refs")]
[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public class UpdateCommand : AsyncCommand<UpdateCommand.Settings>
{
    public sealed class Settings : BaseSettings
    {
        [CommandOption("--index-only")]
        [Description("Only reindex the packages but do not refresh the sources.")]
        public bool IndexOnly { get; init; }
    }

    private readonly IRepositorySourceManager _sourceManager;
    private readonly IPackageIndexer _indexer;

    public UpdateCommand(IRepositorySourceManager sourceManager,
        IPackageIndexer indexer)
    {
        _sourceManager = sourceManager;
        _indexer = indexer;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        if (!settings.IndexOnly)
        {
            var sourceNum = 0;

            foreach (var source in _sourceManager.EnumerateSources())
            {
                sourceNum++;
                await source.RefreshAsync();
            }

            Log.Information("Refreshed {SourceNum} sources", sourceNum);
        }

        // ReSharper disable once InvertIf
        // Justification: ICacheClearable is a special case
        if (_indexer is ICacheClearable clearable)
        {
            Log.Information("Updating packages index");

            clearable.ClearCache();
            await _indexer.IndexAsync(_sourceManager);
        }

        return 0;
    }
}
