namespace IceCraft.Frontend.Commands;

using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using IceCraft.Core.Archive.Artefacts;
using IceCraft.Core.Archive.Indexing;
using IceCraft.Core.Archive.Repositories;
using IceCraft.Core.Network;
using IceCraft.Frontend.Cli;
using Semver;
using Spectre.Console;

public class DownloadCommandFactory : ICommandFactory
{
    private static readonly Argument<string> ArgPackage = new("package", "The package to download.");
    private static readonly Argument<string?> ArgTarget = new("target", () => null, "The file or directory to download to");

    private static readonly Option<string?> OptVersion = new(["-v", "--version"], 
        () => null,
        "The version to download");

    private static readonly Option<bool> OptPrerelease = new("--include-prerelease", "Includes prerelease version");

    private readonly IRepositorySourceManager _sourceManager;
    private readonly IPackageIndexer _indexer;
    private readonly IDownloadManager _downloadManager;
    private readonly IMirrorSearcher _mirrorSearcher;

    public DownloadCommandFactory(IRepositorySourceManager sourceManager,
        IPackageIndexer indexer,
        IDownloadManager downloadManager,
        IMirrorSearcher mirrorSearcher)
    {
        _sourceManager = sourceManager;
        _indexer = indexer;
        _downloadManager = downloadManager;
        _mirrorSearcher = mirrorSearcher;
    }

    public Command CreateCommand()
    {
        var command = new Command("download")
        {
            ArgPackage,
            ArgTarget,
            OptVersion,
            OptPrerelease
        };
        
        command.SetHandler(Execute);
        return command;
    }

    private async Task Execute(InvocationContext context)
    {
        var package = context.GetArgNotNull(ArgPackage);
        var target = context.GetArg(ArgTarget);
        
        var optVersion = context.GetOpt(OptVersion);
        var optPrerelease = context.GetOpt(OptPrerelease);
        
        Output.Shared.Log("Indexing packages");
        var stopwatch = Stopwatch.StartNew();
        var index = await _indexer.IndexAsync(_sourceManager);
        stopwatch.Stop();
        Output.Shared.Verbose("Indexing packages took {0} milliseconds", stopwatch.ElapsedMilliseconds);

        if (!index.TryGetValue(package, out var result))
        {
            Output.Shared.Error("Package series {0} not found", package);
            context.ExitCode = ExitCodes.PackageNotFound;
            return;
        }

        SemVersion? selectedVersion;
        if (!string.IsNullOrWhiteSpace(optVersion))
        {
            // Parse user input, and store in specifiedVersion.
            // Does not need to be that strict on user input since we all make mistakes.
            selectedVersion = SemVersion.Parse(optVersion, SemVersionStyles.Any);
        }
        else
        {
            selectedVersion = await Task.Run(() => result.Versions.GetLatestSemVersion(optPrerelease));
        }

        if (!result.Versions.TryGetValue(selectedVersion.ToString(), out var versionInfo))
        {
            Output.Shared.Error("Version {0} not found for package series {1}", selectedVersion, package);
            context.ExitCode = ExitCodes.PackageNotFound;
            return;
        }

        // Get the best mirror.
        Output.Shared.Verbose("Searching best mirror for {0} ({1})...", 
            versionInfo.Metadata.Id, 
            versionInfo.Metadata.Version);

        var bestMirror = await _mirrorSearcher.GetBestMirrorAsync(versionInfo.Mirrors);
        if (bestMirror != null)
        {
            context.ExitCode = await MirrorDownload(target, bestMirror);
            return;
        }

        Output.Shared.Error("No mirror is available");
        context.ExitCode = ExitCodes.GenericError;
    }
    
    private async Task<int> MirrorDownload(string? destination, ArtefactMirrorInfo bestMirror)
    {
        // Probe for the target directory.
        string targetPath;
        var localFileName = Path.GetFileName(bestMirror.DownloadUri.LocalPath);
        if (string.IsNullOrWhiteSpace(destination))
        {
            Output.Shared.Verbose("download: Using current directory");
            targetPath = Path.Combine(Directory.GetCurrentDirectory(), localFileName);
        }
        else if (Directory.Exists(destination))
        {
            Output.Shared.Verbose("download: Using specified directory name");
            targetPath = Path.Combine(destination, localFileName);
        }
        else
        {
            Output.Shared.Verbose("download: Using specified file name");
            targetPath = destination;
        }

        Output.Shared.Verbose("download: Downloading to {0}", targetPath);

        await AnsiConsole.Progress()
            .StartAsync(async ctx =>
            {
                var task = ctx.AddTask("Download");

                await _downloadManager.DownloadAsync(bestMirror.DownloadUri,
                    targetPath,
                    new SpectreProgressedTask(task));
            });
        
        return 0;
    }
}