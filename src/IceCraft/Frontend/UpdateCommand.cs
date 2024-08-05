namespace IceCraft.Frontend;

using System.ComponentModel;
using IceCraft.Core;
using IceCraft.Core.Archive.Indexing;
using IceCraft.Core.Archive.Repositories;
using IceCraft.Core.Caching;
using JetBrains.Annotations;
using Serilog;
using Spectre.Console.Cli;

[Description("Regenerate all package refs")]
[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public class UpdateCommand : AsyncCommand<BaseSettings>
{
    private readonly IRepositorySourceManager _sourceManager;
    private readonly IPackageIndexer _indexer;
    
    public UpdateCommand(IRepositorySourceManager sourceManager,
        IPackageIndexer indexer)
    {
        _sourceManager = sourceManager;
        _indexer = indexer;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, BaseSettings settings)
    {
        var sourceNum = 0;
        var pkgNum = 0;
        foreach (var source in _sourceManager.EnumerateSources())
        {
            sourceNum++;
            await source.RefreshAsync();

            // So that cache is regenerated.
            var repo = await source.CreateRepositoryAsync();
            if (repo == null)
            {
                continue;
            }

            foreach (var series in repo.EnumerateSeries())
            {
                // Also regenerate latest if they are ever cached.
                _ = await series.GetLatestAsync();

                pkgNum++;
            }
        }

        Log.Information("Refreshed {PkgNum} package series from {SourceNum} sources", pkgNum, sourceNum);

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
