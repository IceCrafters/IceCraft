namespace IceCraft.Frontend.Commands;

using System.ComponentModel;
using IceCraft.Core.Archive.Artefacts;
using IceCraft.Core.Archive.Checksums;
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
    private readonly IChecksumRunner _checksumRunner;

    private readonly record struct QueuedDownloadTask
    {
        internal required Task<string> Task { get; init; }
        internal required RemoteArtefact ArtefactInfo { get; init; }
        internal required PackageMeta Metadata { get; init; }
    }

    public InstallCommand(IPackageInstallManager installManager,
        IPackageIndexer indexer,
        IRepositorySourceManager sourceManager,
        IDownloadManager downloadManager,
        IDependencyResolver dependencyResolver,
        IFrontendApp frontend,
        IChecksumRunner checksumRunner)
    {
        _installManager = installManager;
        _indexer = indexer;
        _sourceManager = sourceManager;
        _downloadManager = downloadManager;
        _dependencyResolver = dependencyResolver;
        _frontend = frontend;
        _checksumRunner = checksumRunner;
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
        // Step 1: Index
        CachedPackageSeriesInfo? seriesInfo = null;
        SemVersion? selectedVersion = null;
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

                var versionInfo = seriesInfo.Versions[selectedVersion.ToString()];
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

        Log.Information("Beginning download");

        // Step 3: download artefacts

        QueuedDownloadTask[] artefactTasks = [];
        await AnsiConsole.Progress()
            .HideCompleted(true)
            .StartAsync(async x =>
            {
                var artefactList = new List<QueuedDownloadTask>(allPackagesSet.Count);

                foreach (var package in allPackagesSet)
                {
                    var task = x.AddTask(package.Id);
                    var versionInfo = index!.GetPackageInfo(package);

                    artefactList.Add(new QueuedDownloadTask()
                    {
                        Task = _downloadManager.DownloadTemporaryArtefactAsync(versionInfo,
                            new SpectreProgressedTask(task),
                            $"{meta!.Id} {meta.Version}"),
                        Metadata = package,
                        ArtefactInfo = versionInfo.Artefact
                    });
                }

                artefactTasks = [.. artefactList];

                await Task.WhenAll(artefactTasks.Select(x => x.Task)).ConfigureAwait(false);
            });

        // Step 4: install artefacts

        await AnsiConsole.Status()
            .StartAsync("Installing packages",
            async context =>
            {
                await _installManager.BulkInstallAsync(ValidateAndInsertInternalAsync(context, artefactTasks), 
                    artefactTasks.Length);
            });

        return 0;
    }

    private async IAsyncEnumerable<KeyValuePair<PackageMeta, string>> ValidateAndInsertInternalAsync(StatusContext status,
        IEnumerable<QueuedDownloadTask> tasks)
    {
        foreach (var task in tasks)
        {
            status.Status($"Installing package {task.Metadata.Id}");

            var path = await task.Task;
            if (!await _checksumRunner.ValidateLocal(task.ArtefactInfo, path))
            {
                Log.Verbose("Remote hash: {Checksum}", task.ArtefactInfo.Checksum);
                throw new KnownException("Artefact hash mismatches downloaded file.");
            }
            yield return new KeyValuePair<PackageMeta, string>(task.Metadata, path);
        }
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
