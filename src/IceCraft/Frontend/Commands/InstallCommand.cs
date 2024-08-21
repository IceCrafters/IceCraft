namespace IceCraft.Frontend.Commands;

using System.ComponentModel;
using IceCraft.Core.Archive.Indexing;
using IceCraft.Core.Archive.Packaging;
using IceCraft.Core.Archive.Repositories;
using IceCraft.Core.Installation;
using IceCraft.Core.Network;
using JetBrains.Annotations;
using Semver;
using Serilog;
using Spectre.Console;
using Spectre.Console.Cli;

[UsedImplicitly]
public class InstallCommand : AsyncCommand<InstallCommand.Settings>
{
    private readonly IPackageInstallManager _installManager;
    private readonly IPackageIndexer _indexer;
    private readonly IRepositorySourceManager _sourceManager;
    private readonly IDownloadManager _downloadManager;

    public InstallCommand(IPackageInstallManager installManager,
        IPackageIndexer indexer,
        IRepositorySourceManager sourceManager,
        IDownloadManager downloadManager)
    {
        _installManager = installManager;
        _indexer = indexer;
        _sourceManager = sourceManager;
        _downloadManager = downloadManager;
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
        Log.Information("Indexing remote packages");
        var index = await _indexer.IndexAsync(_sourceManager);
        if (!index.TryGetValue(settings.PackageName, out var seriesInfo))
        {
            throw new ArgumentException($"No such package series {settings.PackageName}", nameof(settings));
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
            selectedVersion = await Task.Run(() => seriesInfo.Versions.GetLatestSemVersion(settings.IncludePrerelease));
        }

        var versionInfo = seriesInfo.Versions[selectedVersion.ToString()];
        var meta = versionInfo.Metadata;

        // Check if the package is already installed, and if the selected version matches.
        // If all conditions above are true, do not need to do anything.
        if (await _installManager.IsInstalledAsync(meta.Id)
            && !await ComparePackageAsync(meta))
        {
            return 0;
        }

        Log.Information("Beginning download");
        string? fileName = null;

        // TODO origin & questionable
        await AnsiConsole.Progress()
            .StartAsync(async x =>
            {
                var task = x.AddTask("Download");
                fileName = await _downloadManager.DownloadTemporaryArtefactSecureAsync(versionInfo, new SpectreProgressedTask(task));
            });

        if (string.IsNullOrEmpty(fileName))
        {
            throw new InvalidOperationException("Failed to acquire artefact: Unable to get temporary artefact path.");
        }
        await _installManager.InstallAsync(meta, fileName);

        return 0;
    }

    private async Task<bool> ComparePackageAsync(PackageMeta meta)
    {
        // ReSharper disable once InvertIf
        if (await _installManager.IsInstalledAsync(meta.Id, meta.Version.ToString()))
        {
            Log.Information("Package {Id} ({Version}) is already installed.", meta.Id, meta.Version);
            return false;
        }

        return true;
    }

    public class Settings : BaseSettings
    {
        [CommandArgument(0, "<PACKAGE>")]
        [Description("Package to install.")]
        public required string PackageName { get; init; }

        [CommandOption("-v|--version")]
        [Description("Version to install. If unspecified, the latest one is installed.")]
        public string? Version { get; init; }

        [CommandOption("-P|--include-prerelease")]
        [Description("Whether to include prerelease when getting the latest version. Does not affect '--version'.")]
        public bool IncludePrerelease { get; init; }
    }
}
