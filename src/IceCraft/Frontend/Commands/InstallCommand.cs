namespace IceCraft.Frontend.Commands;

using System.ComponentModel;
using IceCraft.Core.Archive.Dependency;
using IceCraft.Core.Archive.Indexing;
using IceCraft.Core.Archive.Packaging;
using IceCraft.Core.Archive.Repositories;
using IceCraft.Core.Installation;
using IceCraft.Core.Network;
using IceCraft.Core.Platform;
using IceCraft.Core.Util;
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
    private readonly IDependencyResolver _dependencyResolver;
    private readonly IFrontendApp _frontend;

    public InstallCommand(IPackageInstallManager installManager,
        IPackageIndexer indexer,
        IRepositorySourceManager sourceManager,
        IDownloadManager downloadManager,
        IDependencyResolver dependencyResolver,
        IFrontendApp frontend)
    {
        _installManager = installManager;
        _indexer = indexer;
        _sourceManager = sourceManager;
        _downloadManager = downloadManager;
        _dependencyResolver = dependencyResolver;
        _frontend = frontend;
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
        // Step 1
        CachedPackageSeriesInfo? seriesInfo = null;
        SemVersion? selectedVersion = null;
        CachedPackageInfo? versionInfo = null;
        PackageMeta? meta = null;
        PackageIndex? index = null;
        HashSet<PackageMeta> allPackagesSet = [];

        await AnsiConsole.Status()
            .StartAsync("Indexing remote packages",
            async context =>
            {
                // STEP: Index remote packages
                index = await _indexer.IndexAsync(_sourceManager);
                if (!index.TryGetValue(settings.PackageName, out seriesInfo))
                {
                    throw new KnownException($"No such package series {settings.PackageName}");
                }

                // STEP: Select version
                context.Status("Acquiring version information");
                if (!string.IsNullOrWhiteSpace(settings.Version))
                {
                    // Parse user input, and store in specifiedVersion.
                    // Does not need to be that strict on user input since we all make mistakes.
                    selectedVersion = SemVersion.Parse(settings.Version, SemVersionStyles.Any);
                }
                else
                {
                    selectedVersion = await Task.Run(() => seriesInfo!.Versions.GetLatestSemVersion(settings.IncludePrerelease));
                }

                versionInfo = seriesInfo.Versions[selectedVersion.ToString()];
                meta = versionInfo.Metadata;

                // Check if the package is already installed, and if the selected version matches.
                // If all conditions above are true, do not need to do anything.
                if (await _installManager.IsInstalledAsync(meta!.Id)
                    && !await ComparePackageAsync(meta))
                {
                    throw new OperationCanceledException();
                }

                // STEP: Resolve all dependencies.
                context.Status("Resolving dependencies");
                allPackagesSet.Add(meta);
                await _dependencyResolver.ResolveTree(meta, index!, allPackagesSet, _frontend.GetCancellationToken());
            });

        // Step 2: Confirmation
        AnsiConsole.MarkupLineInterpolated($":star: Total {allPackagesSet.Count} packages");
        AnsiConsole.Write(new Columns(allPackagesSet.Select(p => p.Id)));

        if (!AnsiConsole.Confirm("Install packages?", defaultValue: false))
        {
            return 0;
        }

        // TODO implement multidownload
        throw new NotImplementedException();

        Log.Information("Beginning download");
        string? fileName = null;

        // TODO origin & questionable
        await AnsiConsole.Progress()
            .HideCompleted(true)
            .StartAsync(async x =>
            {
                var task = x.AddTask("Download");
                fileName = await _downloadManager.DownloadTemporaryArtefactSecureAsync(versionInfo!,
                    new SpectreProgressedTask(task),
                    $"{meta!.Id} {meta.Version}");
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
