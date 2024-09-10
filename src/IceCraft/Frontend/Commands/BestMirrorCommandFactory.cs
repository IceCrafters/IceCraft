namespace IceCraft.Frontend.Commands;

using System.CommandLine;
using System.CommandLine.Invocation;
using IceCraft.Api.Archive.Indexing;
using IceCraft.Api.Archive.Repositories;
using IceCraft.Api.Network;
using IceCraft.Core.Archive.Indexing;
using IceCraft.Core.Archive.Repositories;
using IceCraft.Core.Network;
using IceCraft.Frontend.Cli;

public class BestMirrorCommandFactory
{
    private static readonly Argument<string> ArgPackage = new("package");
    private static readonly Option<bool> OptPrerelease = new(["-P", "--include-prerelease"], "Include prerelease versions when looking for latest version");

    private readonly IPackageIndexer _indexer;
    private readonly IRepositorySourceManager _sourceManager;
    private readonly IMirrorSearcher _searcher;

    public BestMirrorCommandFactory(IPackageIndexer indexer, IRepositorySourceManager sourceManager, IMirrorSearcher searcher)
    {
        _indexer = indexer;
        _sourceManager = sourceManager;
        _searcher = searcher;
    }

    public Command CreateCli()
    {
        var command = new Command("best-mirror", "Find the best mirror for the given package")
        {
            ArgPackage,
            OptPrerelease
        };
        
        command.SetHandler(ExecuteAsync);
        return command;
    }

    private async Task<int> ExecuteAsync(InvocationContext context)
    {
        var package = context.GetArgNotNull(ArgPackage);
        var prerelease = context.GetOpt(OptPrerelease);
        
        var index = await _indexer.IndexAsync(_sourceManager, context.GetCancellationToken());
        if (!index.TryGetValue(package, out var result))
        {
            Output.Shared.Error("Package series {0} not found", package);
            return ExitCodes.PackageNotFound;
        }

        var selectedVersion = await Task.Run(() => result.Versions.GetLatestSemVersion(prerelease));
        var versionInfo = result.Versions[selectedVersion.ToString()];

        var bestMirror = await _searcher.GetBestMirrorAsync(versionInfo.Mirrors);
        if (bestMirror == null)
        {
            Output.Shared.Log("No best mirror");
            return ExitCodes.Ok;
        }

        Output.Shared.Log("Best mirror: {0}", bestMirror.Name);
        return ExitCodes.Ok;
    }
}