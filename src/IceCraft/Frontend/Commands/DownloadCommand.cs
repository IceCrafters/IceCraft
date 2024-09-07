namespace IceCraft.Frontend.Commands;

using System.ComponentModel;
using System.Diagnostics;
using IceCraft.Core.Archive.Artefacts;
using IceCraft.Core.Archive.Indexing;
using IceCraft.Core.Archive.Repositories;
using IceCraft.Core.Network;
using IceCraft.Core.Util;
using JetBrains.Annotations;
using Semver;
using Serilog;
using Spectre.Console;
using Spectre.Console.Cli;

public class DownloadCommand : AsyncCommand<DownloadCommand.Settings>
{
    private readonly IRepositorySourceManager _sourceManager;
    private readonly IPackageIndexer _indexer;
    private readonly IDownloadManager _downloadManager;
    private readonly IMirrorSearcher _mirrorSearcher;

    public DownloadCommand(IRepositorySourceManager sourceManager,
        IPackageIndexer indexer,
        IDownloadManager downloadManager,
        IMirrorSearcher mirrorSearcher)
    {
        _sourceManager = sourceManager;
        _indexer = indexer;
        _downloadManager = downloadManager;
        _mirrorSearcher = mirrorSearcher;
    }

    public override ValidationResult Validate(CommandContext context, Settings settings)
    {
        if (!string.IsNullOrWhiteSpace(settings.Version)
             // Does not need to be that strict on user input since we all make mistakes.
             && !SemVersion.TryParse(settings.Version, SemVersionStyles.Any, out _))
        {
            return ValidationResult.Error("Invalid semantic version");
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

        if (!index.TryGetValue(settings.Package, out var result))
        {
            Log.Error("Package series {PackageId} not found", settings.Package);
            return -2;
        }

        SemVersion? selectedVersion = null;
        if (!string.IsNullOrWhiteSpace(settings.Version))
        {
            // Parse user input, and store in specifiedVersion.
            // Does not need to be that strict on user input since we all make mistakes.
            selectedVersion = SemVersion.Parse(settings.Version, SemVersionStyles.Any);
        }
        else
        {
            selectedVersion = await Task.Run(() => result.Versions.GetLatestSemVersion(settings.IncludePrerelease));
        }

        if (!result.Versions.TryGetValue(selectedVersion.ToString(), out var versionInfo))
        {
            Log.Error("Version {TargetVersion} not found for package series {PackageId}", selectedVersion, settings.Package);
            return -2;
        }

        // Get the best mirror.
        Output.Shared.Verbose("Searching best mirror for {0} ({1})...", 
            versionInfo.Metadata.Id, 
            versionInfo.Metadata.Version);

        var bestMirror = await _mirrorSearcher.GetBestMirrorAsync(versionInfo.Mirrors);
        if (bestMirror != null)
        {
            return await MirrorDownload(settings.Target, bestMirror);
        }

        Log.Fatal("No best mirror available");
        return 1;
    }

    private async Task<int> MirrorDownload(string? destination, ArtefactMirrorInfo bestMirror)
    {
         // Probe for the target directory.
        string targetPath;
        var localFileName = Path.GetFileName(bestMirror.DownloadUri.LocalPath);
        if (string.IsNullOrWhiteSpace(destination))
        {
            Log.Debug("download: Using current directory");
            targetPath = Path.Combine(Directory.GetCurrentDirectory(), localFileName);
        }
        else if (Directory.Exists(destination))
        {
            Log.Debug("download: Using specified directory name");
            targetPath = Path.Combine(destination, localFileName);
        }
        else
        {
            Log.Debug("download: Using specified file name");
            targetPath = destination;
        }

        Log.Debug("download: Downloading to {TargetPath}", targetPath);

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
    
    public class Settings : BaseSettings
    {
        [CommandArgument(0, "<PACKAGE>")]
        [Description("The package series to download artefact of.")]
        [UsedImplicitly]
        public required string Package { get; init; }

        [CommandArgument(1, "[TARGET]")]
        [Description("The target path. If not specified, download to the current working directory. Can specify either file name or directory name.")]
        [UsedImplicitly]
        public string? Target { get; init; }

        [CommandOption("-s|--source")]
        [Description("Gets the source to use.")]
        [UsedImplicitly]
        public string? Source { get; init; }

        [CommandOption("-v|--version")]
        [Description("The target version. If not specified, the latest one is downloaded.")]
        [UsedImplicitly]
        public string? Version { get; init; }

        [CommandOption("-P|--include-prerelease")]
        [Description("Whether to include prerelease when getting the latest version. Does not affect '--version'.")]
        [UsedImplicitly]
        public bool IncludePrerelease { get; init; }
    }
}
