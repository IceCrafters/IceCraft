#if LEGACY_INTERFACE
namespace IceCraft.Frontend.Commands;

using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using IceCraft.Core.Archive.Dependency;
using IceCraft.Core.Archive.Indexing;
using IceCraft.Core.Archive.Packaging;
using IceCraft.Core.Archive.Repositories;
using IceCraft.Frontend;
using JetBrains.Annotations;
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
        var stopwatch = Stopwatch.StartNew();
        var index = await _indexer.IndexAsync(_sourceManager);
        stopwatch.Stop();

        if (!index.TryGetValue(settings.PackageId, out var result))
        {
            Output.Shared.Error("Package series {PackageId} not found", settings.PackageId);
            return -2;
        }

        var latestId = await Task.Run(() => result.Versions.GetLatestSemVersion(settings.IncludePrerelease));

        if (latestId == null
            || !result.Versions.TryGetValue(latestId.ToString(), out var latest))
        {
            Output.Shared.Warning("Package did not specify latest version");
            return 0;
        }

        var meta = latest.Metadata;

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLineInterpolated($"[aqua underline]{meta.Id}[/] [grey]ver.[/] [teal]{meta.Version}[/]");
        AnsiConsole.MarkupLineInterpolated($"[silver]Released: [/]{meta.ReleaseDate}");
        AnsiConsole.MarkupLineInterpolated($"[silver]Unitary: [/]{meta.Unitary}");

        if (meta.Dependencies != null)
        {
            AnsiConsole.Markup("[silver]Dependencies: [/]");
            PrintRefs(meta.Dependencies);
            AnsiConsole.WriteLine();
        }

        if (meta.ConflictsWith != null)
        {
            AnsiConsole.Markup("[silver]Conflicts with: [/]");
            PrintRefs(meta.ConflictsWith);
            AnsiConsole.WriteLine();
        }

        AnsiConsole.WriteLine();

        // Transcript
        if (meta.Transcript != null)
        {
            if (meta.Transcript.Authors.Count > 0)
            {
                string authorsString = GetAuthors(meta);
                AnsiConsole.MarkupLineInterpolated($"[silver]By:[/] [steelblue]{authorsString}[/]");
            }

            if (!meta.Transcript.Maintainer.Equals(default(PackageAuthorInfo)))
            {
                AnsiConsole.MarkupLineInterpolated($"[silver]Artefact Maintainer:[/] [cadetblue]{meta.Transcript.Maintainer}[/]");
            }

            if (!meta.Transcript.PluginMaintainer.Equals(default(PackageAuthorInfo)))
            {
                AnsiConsole.MarkupLineInterpolated($"[silver]Plugin Maintainer:[/] [cadetblue]{meta.Transcript.PluginMaintainer}[/]");
            }

            if (meta.Transcript.License != null)
            {
                AnsiConsole.MarkupLineInterpolated($"[silver]License:[/] [cadetblue]{meta.Transcript.License}[/]");
            }

            if (meta.Transcript.Description != null)
            {
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLineInterpolated($"[white]{meta.Transcript.Description}[/]");
                AnsiConsole.WriteLine();
            }
        }

        return 0;
    }

    private static void PrintRefs(DependencyCollection dependencies)
    {
        var firstDependency = false;
        foreach (var depRef in dependencies)
        {
            if (firstDependency)
            {
                AnsiConsole.Markup("[grey],[/] ");
            }
            firstDependency = true;

            AnsiConsole.MarkupInterpolated($"[plum4]{depRef.PackageId}[/] [silver]([/][grey53]{depRef.VersionRange}[/][silver])[/]");
        }
    }

    private static string GetAuthors(PackageMeta meta)
    {
        var builder = new StringBuilder();

        if (meta.Transcript == null)
        {
            return "(no authors)";
        }

        // Assemble authors string
        var authorFirst = false;
        foreach (var author in meta.Transcript.Authors)
        {
            if (authorFirst)
            {
                builder.Append(", ");
            }

            builder.Append(author.ToString());
            authorFirst = true;
        }

        var authorsString = builder.ToString();
        return authorsString;
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
#endif