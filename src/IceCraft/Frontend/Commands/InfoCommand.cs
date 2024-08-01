namespace IceCraft.Frontend.Commands;

using System.ComponentModel;
using System.Diagnostics;
using IceCraft.Core;
using IceCraft.Core.Archive.Indexing;
using IceCraft.Frontend;
using Serilog;
using Spectre.Console;
using Spectre.Console.Cli;

public class InfoCommand : AsyncCommand<InfoCommand.Settings>
{
    private readonly IRepositorySourceManager _sourceManager;
    private readonly IPackageIndexer _indexer;

    public InfoCommand(IRepositorySourceManager sourceManager,
        IPackageIndexer indexer)
    {
        _sourceManager = sourceManager;
        _indexer = indexer;
    }

    public override ValidationResult Validate(CommandContext context, Settings settings)
    {
        if (settings.Source != null && !_sourceManager.ContainsSource(settings.Source))
        {
            return ValidationResult.Error($"No such source: {settings.Source}");
        }

        return base.Validate(context, settings);
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        Log.Information("Indexing packages");
        var stopwatch = Stopwatch.StartNew();
        var index = await _indexer.IndexAsync(_sourceManager);
        stopwatch.Stop();
        Log.Verbose("Indexing packages took {ElapsedMilliseconds} milliseconds", stopwatch.ElapsedMilliseconds);

        var result = index[settings.PackageId];

        if (result == null)
        {
            Log.Error("Package series {PackageId} not found", settings.PackageId);
            return -2;
        }

        Log.Information("Package ID: {PackageId}", result.Id);
        Log.Information("Latest version release: {ReleaseDate}", result.ReleaseDate);
        Log.Information("Latest version number: {Version}", result.Version);
        return 0;
    }

    public class Settings : BaseSettings
    {
        [CommandArgument(0, "<PACKAGE>")]
        public required string PackageId { get; init; }

        [CommandOption("-s|--source")]
        [Description("The source to inspect package from.")]
        public string? Source { get; init; }
    }
}
