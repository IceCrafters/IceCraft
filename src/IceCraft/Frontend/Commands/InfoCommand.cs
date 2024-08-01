namespace IceCraft.Frontend.Commands;

using System.ComponentModel;
using IceCraft.Core;
using IceCraft.Core.Archive;
using IceCraft.Frontend;
using Serilog;
using Spectre.Console;
using Spectre.Console.Cli;

public class InfoCommand : AsyncCommand<InfoCommand.Settings>
{
    private readonly IRepositorySourceManager _sourceManager;

    public InfoCommand(IRepositorySourceManager sourceManager)
    {
        _sourceManager = sourceManager;
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
        var repos = await _sourceManager.GetRepositories();

        IPackageSeries? result = null;

        foreach (var repo in repos)
        {
            result = repo.GetSeriesOrDefault(settings.PackageId);
            if (result == null)
            {
                continue;
            }
            break;
        }

        if (result == null)
        {
            Log.Error("Package series {PackageId} not found", settings.PackageId);
            return -2;
        }

        var latest = await result.GetLatestAsync();
        Log.Information("Package ID: {PackageId}", result.Name);

        if (latest == null)
        {
            Log.Information("Package series {Name} doesn't have a latest version", result.Name);
            return 0;
        }

        var meta = latest.GetMeta();
        Log.Information("Latest version release: {ReleaseDate}", meta.ReleaseDate);
        Log.Information("Latest version number: {Version}", meta.Version);
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
