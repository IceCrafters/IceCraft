namespace IceCraft.Frontend.Commands;

using System.ComponentModel;
using IceCraft.Core.Archive.Artefacts;
using IceCraft.Core.Archive.Checksums;
using IceCraft.Core.Archive.Dependency;
using IceCraft.Core.Archive.Indexing;
using IceCraft.Core.Archive.Packaging;
using IceCraft.Core.Archive.Repositories;
using IceCraft.Core.Caching;
using IceCraft.Core.Installation;
using IceCraft.Core.Installation.Analysis;
using IceCraft.Core.Network;
using IceCraft.Core.Platform;
using IceCraft.Core.Util;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
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
    private readonly IDependencyMapper _dependencyMapper;
    private readonly IArtefactManager _artefactManager;

    private readonly record struct QueuedDownloadTask
    {
        internal required Task<DownloadResult> Task { get; init; }
        internal required RemoteArtefact ArtefactInfo { get; init; }
        internal required PackageMeta Metadata { get; init; }
        internal required string Objective { get; init; }
        internal Stream? Stream { get; init; }
    }

    public InstallCommand(IServiceProvider serviceProvider)
    {
        _installManager = serviceProvider.GetRequiredService<IPackageInstallManager>();
        _indexer = serviceProvider.GetRequiredService<IPackageIndexer>();
        _sourceManager = serviceProvider.GetRequiredService<IRepositorySourceManager>();
        _downloadManager = serviceProvider.GetRequiredService<IDownloadManager>();
        _dependencyResolver = serviceProvider.GetRequiredService<IDependencyResolver>();
        _frontend = serviceProvider.GetRequiredService<IFrontendApp>();
        _checksumRunner = serviceProvider.GetRequiredService<IChecksumRunner>();
        _dependencyMapper = serviceProvider.GetRequiredService<IDependencyMapper>();
        _artefactManager = serviceProvider.GetRequiredService<IArtefactManager>();
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
        // Step 0: Clean artefacts
        if (!settings.NoCleanArtefact)
        {
            AnsiConsole.Status()
                .Start("Cleaning artefacts", _ => _artefactManager.CleanArtefacts());
        }

        // Step 1: Index
        SemVersion? selectedVersion;
        PackageMeta? meta = null;
        PackageIndex? index = null;
        HashSet<PackageMeta> allPackagesSet = [];

        await AnsiConsole.Status()
            .StartAsync("Indexing remote packages",
                async ctx =>
                {
                    // STEP: Index remote packages
                    index = await _indexer.IndexAsync(_sourceManager);
                    if (!index.TryGetValue(settings.PackageName, out var seriesInfo))
                    {
                        throw new KnownException($"No such package series {settings.PackageName}");
                    }

                    // STEP: Select version
                    ctx.Status("Acquiring version information");
                    if (!string.IsNullOrWhiteSpace(settings.Version))
                    {
                        // Parse user input, and store in specifiedVersion.
                        // Does not need to be that strict on user input since we all make mistakes.
                        selectedVersion = SemVersion.Parse(settings.Version, SemVersionStyles.Any);
                    }
                    else
                    {
                        selectedVersion = await Task.Run(() =>
                            seriesInfo.Versions.GetLatestSemVersion(settings.IncludePrerelease));
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
                    ctx.Status("Resolving dependencies");
                    allPackagesSet.Add(meta);
                    await _dependencyResolver.ResolveTree(meta, index!, allPackagesSet,
                        _frontend.GetCancellationToken());

                    ctx.Status("Checking for conflicts");
                    foreach (var package in allPackagesSet)
                    {
                        if (!await _installManager.CheckForConflictAsync(package))
                        {
                            throw new KnownException("Package conflict detected.");
                        }
                    }
                });

        // Step 2: Confirmation
        AnsiConsole.MarkupLineInterpolated($":star: Total {allPackagesSet.Count} packages");
        AnsiConsole.Write(new Columns(allPackagesSet.Select(p => p.Id)));

        if (!AnsiConsole.Confirm("Install packages?", defaultValue: false))
        {
            return 0;
        }

        _frontend.Output.Verbose("Initializing download for {0} packages", allPackagesSet.Count);

        // Step 3: download artefacts

        QueuedDownloadTask[] artefactTasks = [];
        await AnsiConsole.Progress()
            .HideCompleted(true)
            .StartAsync(async ctx =>
            {
                var artefactList = new List<QueuedDownloadTask>(allPackagesSet.Count);
                foreach (var package in allPackagesSet)
                {
                    var packageInfo = index!.GetPackageInfo(package);

                    var artefactFile = await _artefactManager.GetSafeArtefactPathAsync(packageInfo.Artefact, 
                        packageInfo.Metadata);

                    // Detect existing artefacts.
                    if (artefactFile != null)
                    {
                        artefactList.Add(new QueuedDownloadTask()
                        {
                            Task = Task.FromResult(DownloadResult.Succeeded),
                            ArtefactInfo = packageInfo.Artefact,
                            Metadata = packageInfo.Metadata,
                            Objective = artefactFile
                        }
                        );

                        continue;
                    }

                    // Download new artefact.
                    var task = ctx.AddTask(package.Id);
                    var stream = _artefactManager.CreateArtefact(packageInfo.Artefact,
                        packageInfo.Metadata);

                    artefactList.Add(new QueuedDownloadTask()
                    {
                        Task = _downloadManager.DownloadAsync(packageInfo,
                                stream,
                                new SpectreProgressedTask(task),
                                $"{meta!.Id} {meta.Version}"),
                        ArtefactInfo = packageInfo.Artefact,
                        Metadata = packageInfo.Metadata,
                        Objective = _artefactManager.GetArtefactPath(packageInfo.Artefact,
                            packageInfo.Metadata),
                        Stream = stream
                    }
                    );
                }

                artefactTasks = [.. artefactList];

                await Task.WhenAll(artefactTasks.Select(x => x.Task)).ConfigureAwait(false);
            });

        // Step 4: install artefacts and map dependencies

        await AnsiConsole.Status()
            .StartAsync("Installing packages",
                async ctx =>
                {
                    await _installManager.BulkInstallAsync(ValidateAndInsertInternalAsync(ctx, artefactTasks),
                        artefactTasks.Length);

                    ctx.Status("Evaluating dependency information");

                    if (_dependencyMapper is ICacheClearable clearable)
                    {
                        clearable.ClearCache();
                    }

                    await _dependencyMapper.MapDependenciesCached();
                });

        return 0;
    }

    private async IAsyncEnumerable<KeyValuePair<PackageMeta, string>> ValidateAndInsertInternalAsync(
        StatusContext status,
        IEnumerable<QueuedDownloadTask> tasks)
    {
        foreach (var task in tasks)
        {
            var result = await task.Task;
            if (result == DownloadResult.Failed)
            {
                throw new KnownException($"Download for {task.Metadata.Id} ({task.Metadata.Version}) have FAILED.");
            }

            status.Status($"Installing package {task.Metadata.Id}");

            if (task.Stream != null)
            {
                await task.Stream.DisposeAsync();
            }

            var path = task.Objective;
            if (!await _checksumRunner.ValidateLocal(task.ArtefactInfo, path))
            {
                Output.Shared.Verbose("Remote hash: {0}", task.ArtefactInfo.Checksum);
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
            Output.Shared.Log("Package {0} ({1}) is already installed.", meta.Id, meta.Version);
            return false;
        }

        return true;
    }

    [UsedImplicitly]
    public class Settings : BaseSettings
    {
        [CommandArgument(0, "<PACKAGE>")]
        [Description("Package to install.")]
        [UsedImplicitly]
        public required string PackageName { get; init; }

        [CommandOption("-v|--version")]
        [Description("Version to install. If unspecified, the latest one is installed.")]
        [UsedImplicitly]
        public string? Version { get; init; }

        [CommandOption("-P|--include-prerelease")]
        [Description("Whether to include prerelease when getting the latest version. Does not affect '--version'.")]
        [UsedImplicitly]
        public bool IncludePrerelease { get; init; }

        [CommandOption("--no-clean-artefact")]
        [Description("Do not perform artefact cleaning tasks.")]
        public bool NoCleanArtefact { get; init; }
    }
}