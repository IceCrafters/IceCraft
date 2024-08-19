namespace IceCraft.Frontend.Commands;

using System.ComponentModel;
using System.Diagnostics;
using IceCraft.Core;
using IceCraft.Core.Archive.Indexing;
using IceCraft.Core.Archive.Repositories;
using IceCraft.Frontend;
using JetBrains.Annotations;
using Serilog;
using Spectre.Console;
using Spectre.Console.Cli;

[Description("Displays information about a package series")]
[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
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

        if (!index.TryGetValue(settings.PackageId, out var result))
        {
            Log.Error("Package series {PackageId} not found", settings.PackageId);
            return -2;
        }

        var latestId = await Task.Run(() => result.Versions.GetLatestSemVersion(settings.IncludePrerelease));

        if (latestId == null
            || !result.Versions.TryGetValue(latestId.ToString(), out var latest))
        {
            Log.Warning("Package did not specify latest version");
            return 0;
        }

        var meta = latest.Metadata;

        Log.Information("Package ID: {PackageId}", meta.Id);
        Log.Information("Latest version release: {ReleaseDate}", meta.ReleaseDate);
        Log.Information("Latest version number: {Version}", meta.Version);
        return 0;
    }

    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public class Settings : BaseSettings
    {
        [CommandArgument(0, "<PACKAGE>")]
        public required string PackageId { get; init; }

        [CommandOption("-s|--source")]
        [Description("The source to inspect package from.")]
        public string? Source { get; init; }

        [CommandOption("-P|--include-prerelease")]
        [Description("Whether to include prerelease when getting the latest version. Does not affect '--version'.")]
        public bool IncludePrerelease { get; init; }
    }
}
