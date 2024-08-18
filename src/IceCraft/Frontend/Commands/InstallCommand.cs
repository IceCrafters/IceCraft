namespace IceCraft.Frontend.Commands;

using System.ComponentModel;
using IceCraft.Core.Archive.Indexing;
using IceCraft.Core.Archive.Repositories;
using IceCraft.Core.Installation;
using IceCraft.Core.Network;
using Serilog;
using Spectre.Console;
using Spectre.Console.Cli;

public class InstallCommand : AsyncCommand<InstallCommand.Settings>
{
    private readonly IPackageInstallManager _installManager;
    private readonly IPackageIndexer _indexer;
    private readonly IRepositorySourceManager _sourceManager;
    private readonly IMirrorSearcher _mirrorSearcher;
    private readonly IDownloadManager _downloadManager;

    public InstallCommand(IPackageInstallManager installManager,
        IPackageIndexer indexer,
        IRepositorySourceManager sourceManager,
        IMirrorSearcher mirrorSearcher,
        IDownloadManager downloadManager)
    {
        _installManager = installManager;
        _indexer = indexer;
        _sourceManager = sourceManager;
        _mirrorSearcher = mirrorSearcher;
        _downloadManager = downloadManager;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        Log.Information("Indexing remote packages");
        var index = await _indexer.IndexAsync(_sourceManager);
        if (!index.TryGetValue(settings.PackageName, out var seriesInfo))
        {
            throw new ArgumentException($"No such package series {settings.PackageName}", nameof(settings));
        }

        var selectedVersion = (settings.Version ?? seriesInfo.LatestVersion)
            ?? throw new ArgumentException($"Package series {settings.PackageName} do not have a latest version. Please specify one.", nameof(settings));

        var versionInfo = seriesInfo.Versions[selectedVersion];
        var meta = versionInfo.Metadata;

        Log.Information("Beginning download");
        string? fileName = null;

        // TODO origin & questionable
        await AnsiConsole.Progress()
            .StartAsync(async x =>
            {
                var task = x.AddTask("Download");
                fileName = await _downloadManager.DownloadTemporaryArtefactAsync(versionInfo, new SpectreDownloadTask(task));
            });

        if (string.IsNullOrEmpty(fileName))
        {
            throw new InvalidOperationException("Failed to acquire artefact: Unable to get temporary artefact path.");
        }
        await _installManager.InstallAsync(meta, fileName);

        return 0;
    }

    public class Settings : BaseSettings
    {
        [CommandArgument(0, "<PACKAGE>")]
        [Description("Package to install.")]
        public required string PackageName { get; init; }

        [CommandOption("-v|--version")]
        [Description("Version to install. If unspecified, the latest one is installed.")]
        public string? Version { get; init; }
    }
}
