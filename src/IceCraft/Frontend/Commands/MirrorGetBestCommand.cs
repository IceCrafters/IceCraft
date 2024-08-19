namespace IceCraft.Frontend.Commands;

using System.ComponentModel;
using IceCraft.Core.Archive.Indexing;
using IceCraft.Core.Archive.Repositories;
using IceCraft.Core.Network;
using IceCraft.Core.Platform;
using Semver;
using Serilog;
using Spectre.Console.Cli;

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

        var selectedVersion = await Task.Run(() => result.Versions.GetLatestSemVersion(settings.IncludePrerelease));
        var versionInfo = result.Versions[selectedVersion.ToString()];

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

        [CommandOption("-P|--include-prerelease")]
        [Description("Whether to include prerelease versions")]
        public bool IncludePrerelease { get; init; }
    }
}
