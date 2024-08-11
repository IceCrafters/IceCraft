namespace IceCraft.Frontend.Commands;

using System.ComponentModel;
using System.Diagnostics;
using IceCraft.Core.Archive.Indexing;
using IceCraft.Core.Archive.Repositories;
using IceCraft.Core.Network;
using Serilog;
using Spectre.Console;
using Spectre.Console.Cli;

public class DownloadCommand : AsyncCommand<DownloadCommand.Settings>
{
    private readonly IRepositorySourceManager _sourceManager;
    private readonly IPackageIndexer _indexer;
    private readonly IDownloadManager _downloadManager;

    public DownloadCommand(IRepositorySourceManager sourceManager,
        IPackageIndexer indexer,
        IDownloadManager downloadManager)
    {
        _sourceManager = sourceManager;
        _indexer = indexer;
        _downloadManager = downloadManager;
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

        var targetVersion = settings.Version ?? result.LatestVersion;
        if (string.IsNullOrEmpty(targetVersion))
        {
            Log.Error("Package series {PackageId} does not have latest version. Please specify a version.", settings.Package);
            return -2;
        }
        
        if (!result.Versions.TryGetValue(targetVersion, out var versionInfo))
        {
            Log.Error("Version {TargetVersion} not found for package series {PackageId}", targetVersion, settings.Package);
            return -2;
        }

        // Probe for the target directory.
        string targetPath;
        var localFileName = Path.GetFileName(versionInfo.Artefact.DownloadUri.LocalPath);
        if (string.IsNullOrWhiteSpace(settings.Target))
        {
            Log.Debug("download: Using current directory");
            targetPath = Path.Combine(Directory.GetCurrentDirectory(), localFileName);
        }
        else if (Directory.Exists(settings.Target))
        {
            Log.Debug("download: Using specified directory name");
            targetPath = Path.Combine(settings.Target, localFileName);
        }
        else
        {
            Log.Debug("download: Using specified file name");
            targetPath = settings.Target;
        }

        Log.Debug("download: Downloading to {TargetPath}", targetPath);

        await AnsiConsole.Progress()
            .StartAsync(async ctx =>
            {
                var task = ctx.AddTask("Download");

                await _downloadManager.Download(versionInfo.Artefact.DownloadUri,
                    targetPath,
                    new SpectreDownloadTask(task));
            });
        
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
