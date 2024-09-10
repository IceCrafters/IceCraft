namespace IceCraft.Frontend.Commands;

using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.Text;
using IceCraft.Api.Archive.Indexing;
using IceCraft.Api.Archive.Repositories;
using IceCraft.Api.Installation.Dependency;
using IceCraft.Api.Package;
using IceCraft.Core.Archive.Dependency;
using IceCraft.Core.Archive.Indexing;
using IceCraft.Core.Archive.Repositories;
using IceCraft.Frontend.Cli;
using Spectre.Console;

public class InfoCommandFactory : ICommandFactory
{
    private static readonly Argument<string> ArgPackage = new("package", "The package to lookup");
    private static readonly Option<string?> OptSource = new(["-s", "--source"], "The source to lookup");
    private static readonly Option<bool> OptPrerelease = new(["-P", "--include-prerelease"], "Includes prerelease versions");

    private readonly IPackageIndexer _indexer;
    private readonly IRepositorySourceManager _sourceManager;

    public InfoCommandFactory(IPackageIndexer indexer, 
        IRepositorySourceManager sourceManager)
    {
        _indexer = indexer;
        _sourceManager = sourceManager;
    }

    public Command CreateCommand()
    {
        var command = new Command("info", "Looks up metadata information for a given package")
        {
            ArgPackage,
            OptSource,
            OptPrerelease
        };

        command.SetHandler(ExecuteAsync);
        return command;
    }

    private async Task ExecuteAsync(InvocationContext context)
    {
        var packageId = context.GetArgNotNull(ArgPackage);
        var includePrerelease = context.GetOpt(OptPrerelease);

        var stopwatch = Stopwatch.StartNew();
        var index = await _indexer.IndexAsync(_sourceManager);
        stopwatch.Stop();

        if (!index.TryGetValue(packageId, out var result))
        {
            Output.Shared.Error("Package series {0} not found", packageId);
            context.ExitCode = ExitCodes.PackageNotFound;
            return;
        }

        var latestId = await Task.Run(() => result.Versions.GetLatestSemVersion(includePrerelease));

        if (!result.Versions.TryGetValue(latestId.ToString(), out var latest))
        {
            Output.Shared.Error("Cannot find a latest version");
            context.ExitCode = ExitCodes.PackageNotFound;
            return;
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
                var authorsString = GetAuthors(meta);
                AnsiConsole.MarkupLineInterpolated($"[silver]By:[/] [steelblue]{authorsString}[/]");
            }

            if (!meta.Transcript.Maintainer.Equals(default))
            {
                AnsiConsole.MarkupLineInterpolated($"[silver]Artefact Maintainer:[/] [cadetblue]{meta.Transcript.Maintainer}[/]");
            }

            if (!meta.Transcript.PluginMaintainer.Equals(default))
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
}
