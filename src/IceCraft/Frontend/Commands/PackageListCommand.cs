namespace IceCraft.Frontend.Commands;

using System.ComponentModel;
using System.Threading.Tasks;
using IceCraft.Core.Archive.Indexing;
using IceCraft.Core.Archive.Repositories;
using IceCraft.Core.Platform;
using Serilog;
using Spectre.Console.Cli;

[Description("List all versions for a given package")]
public sealed class PackageListCommand : AsyncCommand<PackageListCommand.Settings>
{
    private readonly IRepositorySourceManager _sourceManager;
    private readonly IPackageIndexer _indexer;
    private readonly IFrontendApp _frontend;

    public PackageListCommand(IRepositorySourceManager sourceManager,
        IPackageIndexer indexer,
        IFrontendApp frontend)
    {
        _sourceManager = sourceManager;
        _indexer = indexer;
        _frontend = frontend;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var index = await _indexer.IndexAsync(_sourceManager, _frontend.GetCancellationToken());

        if (!index.TryGetValue(settings.PackageName, out var seriesInfo))
        {
            Log.Fatal("Package {PackageName} not found", settings.PackageName);
            return -1;
        }

        foreach (var version in seriesInfo.Versions
            .Where(x => !x.Value.Metadata.Version.IsPrerelease || settings.IncludePrerelease))
        {
            Console.WriteLine(version.Key);
        }

        return 0;
    }

    public sealed class Settings : BaseSettings
    {
        [CommandArgument(0, "<PACKAGE>")]
        [Description("The name of the package to list")]
        public required string PackageName { get; init; }

        [CommandOption("-P|--include-prerelease")]
        [Description("Whether to include prerelease when getting the latest version. Does not affect '--version'.")]
        public bool IncludePrerelease { get; init; }
    }
}
