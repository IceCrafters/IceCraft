namespace IceCraft.Frontend.Commands;

using System.ComponentModel;
using System.Diagnostics;
using IceCraft.Core.Archive.Indexing;
using IceCraft.Core.Archive.Repositories;
using Serilog;
using Spectre.Console.Cli;

public class DownloadCommand : AsyncCommand<DownloadCommand.Settings>
{
    private readonly IRepositorySourceManager _sourceManager;
    private readonly IPackageIndexer _indexer;

    public DownloadCommand(IRepositorySourceManager sourceManager,
        IPackageIndexer indexer)
    {
        _sourceManager = sourceManager;
        _indexer = indexer;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        Log.Information("Indexing packages");
        var stopwatch = Stopwatch.StartNew();
        var index = await _indexer.IndexAsync(_sourceManager);
        stopwatch.Stop();
        Log.Verbose("Indexing packages took {ElapsedMilliseconds} milliseconds", stopwatch.ElapsedMilliseconds);

        if (!index.TryGetValue(settings.Package, out var result))
        {
            Log.Error("Package series {PackageId} not found", settings.Package);
            return -2;
        }
        
        // TODO complete implementation
        return 0;
    }

    public class Settings : BaseSettings
    {
        [CommandArgument(0, "<PACKAGE>")]
        [Description("The package series to download artefact of.")]
        public required string Package { get; init; }

        [CommandArgument(1, "[TARGET]")]
        [Description("The target path. If not specified, download to the current working directory. Can specify either file name or directory name.")]
        public string? Target { get; init; }

        [CommandOption("-s|--source")]
        [Description("Gets the source to use.")]
        public string? Source { get; init; }

        [CommandOption("-v|--version")]
        [Description("The target version. If not specified, the latest one is downloaded.")]
        public string? Version { get; init; }
    }
}
