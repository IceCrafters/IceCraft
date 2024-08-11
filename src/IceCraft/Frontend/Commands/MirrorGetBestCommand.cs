using IceCraft.Core.Archive.Indexing;
using IceCraft.Core.Archive.Repositories;
using IceCraft.Core.Network;
using IceCraft.Core.Platform;
using Serilog;
using Spectre.Console.Cli;

namespace IceCraft.Frontend.Commands;

public class MirrorGetBestCommand : AsyncCommand<MirrorGetBestCommand.Settings>
{
    private readonly IRepositorySourceManager _sourceManager;
    private readonly IPackageIndexer _indexer;
    private readonly IMirrorSearcher _searcher;
    private readonly IFrontendApp _frontend;

    public MirrorGetBestCommand(IRepositorySourceManager sourceManager,
        IPackageIndexer indexer,
        IMirrorSearcher searcher,
        IFrontendApp frontend)
    {
        _sourceManager = sourceManager;
        _indexer = indexer;
        _searcher = searcher;
        _frontend = frontend;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var index = await _indexer.IndexAsync(_sourceManager, _frontend.GetCancellationToken());
        if (!index.TryGetValue(settings.PackageSeries, out var result))
        {
            Log.Error("Package series {PackageSeries} not found", settings.PackageSeries);
            return -2;
        }

        var latestId = result.LatestVersion;
        if (latestId == null ||
            !result.Versions.TryGetValue(latestId, out var versionInfo))
        {
            Log.Warning("Cannot acquire latest version. Try specifying a version.");
            return -2;
        }

        var bestMirror = await _searcher.GetBestMirrorAsync(versionInfo.Mirrors);
        if (bestMirror == null)
        {
            Log.Information("No best mirror");
            return 0;
        }

        Log.Information("Best mirror: {Name}", bestMirror.Name);
        return 0;
    }

    public class Settings : BaseSettings
    {
        [CommandArgument(0, "<PACKAGE>")]
        public required string PackageSeries { get; init; }
    }
}
