// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Interactive;

using IceCraft.Api.Archive.Artefacts;
using IceCraft.Api.Archive.Indexing;
using IceCraft.Api.Caching;
using IceCraft.Api.Exceptions;
using IceCraft.Api.Installation;
using IceCraft.Api.Installation.Dependency;
using IceCraft.Api.Network;
using IceCraft.Api.Package;
using IceCraft.Frontend;
using Spectre.Console;

public class InteractiveInstaller
{
    private readonly IDownloadManager _downloadManager;
    private readonly IArtefactManager _artefactManager;
    private readonly IChecksumRunner _checksumRunner;
    private readonly IDependencyMapper _dependencyMapper;
    private readonly IMirrorSearcher _mirrorSearcher;
    private readonly IPackageSetupAgent _setupAgent;

    public InteractiveInstaller(IDownloadManager downloadManager,
        IArtefactManager artefactManager,
        IChecksumRunner checksumRunner,
        IDependencyMapper dependencyMapper,
        IMirrorSearcher mirrorSearcher,
        IPackageSetupAgent setupAgent)
    {
        _downloadManager = downloadManager;
        _artefactManager = artefactManager;
        _checksumRunner = checksumRunner;
        _dependencyMapper = dependencyMapper;
        _mirrorSearcher = mirrorSearcher;
        _setupAgent = setupAgent;
    }

    private readonly record struct QueuedDownloadTask
    {
        internal required Task<DownloadResult> Task { get; init; }
        internal required IArtefactDefinition ArtefactInfo { get; init; }
        internal required PackageMeta Metadata { get; init; }
        internal required string Objective { get; init; }
        internal Stream? Stream { get; init; }
        internal required bool IsExplicit { get; init; }
    }

    public static bool AskConfirmation(ISet<DependencyLeaf> packages)
    {
        AnsiConsole.MarkupLineInterpolated($":star: Total {packages.Count} packages");
        AnsiConsole.Write(new Columns(packages.Select(p => p.Package.Id)));

        return AnsiConsole.Confirm("Install packages?", defaultValue: false);
    }

    public async Task<int> InstallAsync(ISet<DependencyLeaf> packages, PackageIndex index, bool forceRedownload)
    {
        QueuedDownloadTask[] artefactTasks = [];
        await AnsiConsole.Progress()
            .HideCompleted(true)
            .StartAsync(async ctx =>
            {
                var artefactList = new List<QueuedDownloadTask>(packages.Count);

                // Go through every package to install and queue download for them.
                foreach (var package in packages)
                {
                    var packageInfo = index.GetPackageInfo(package.Package);
                    var artefact = packageInfo.Artefact;
                
                    // Handle volatile artefact
                    if (artefact is VolatileArtefact)
                    {
                        // Download volatile artefact.
                        var vTask = ctx.AddTask(package.Package.Id);
                        var vTempFile = Path.Combine(Path.GetTempPath(),
                            Path.GetRandomFileName());
                        var vStream = File.Create(vTempFile);

                        artefactList.Add(new QueuedDownloadTask()
                        {
                            Task = _downloadManager.DownloadArtefactAsync(
                                    _mirrorSearcher,
                                    packageInfo,
                                    vStream,
                                    new SpectreProgressedTask(vTask),
                                    $"{packageInfo.Metadata.Id} {packageInfo.Metadata.Version}"),
                            ArtefactInfo = artefact,
                            Metadata = packageInfo.Metadata,
                            Objective = vTempFile,
                            Stream = vStream,
                            IsExplicit = package.IsExplicit
                        });

                        continue;
                    }

                    if (!forceRedownload)
                    {
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
                                Objective = artefactFile,
                                IsExplicit = package.IsExplicit
                            }
                            );

                            continue;
                        }
                    }

                    var stream = _artefactManager.CreateArtefactFile(packageInfo.Artefact,
                        packageInfo.Metadata);
                                            // Download new artefact.
                    var task = ctx.AddTask(package.Package.Id);

                    artefactList.Add(new QueuedDownloadTask()
                    {
                        Task = _downloadManager.DownloadArtefactAsync(
                                _mirrorSearcher,
                                packageInfo,
                                stream,
                                new SpectreProgressedTask(task),
                                $"{packageInfo.Metadata.Id} {packageInfo.Metadata.Version}"),
                        ArtefactInfo = packageInfo.Artefact,
                        Metadata = packageInfo.Metadata,
                        Objective = _artefactManager.GetArtefactPath(packageInfo.Artefact,
                            packageInfo.Metadata)!,
                        Stream = stream,
                        IsExplicit = package.IsExplicit
                    }
                    );
                }

                artefactTasks = [.. artefactList];

                await Task.WhenAll(artefactTasks.Select(x => x.Task)).ConfigureAwait(false);
            });

        // Step 4: install artefacts and map dependencies

        await _setupAgent.InstallManyAsync(ValidateAndInsertInternalAsync(artefactTasks),
            artefactTasks.Length);

        AnsiConsole.MarkupLine("[bold white]:gear:[/] [deepskyblue1]Resolve dependencies[/]");

        if (_dependencyMapper is ICacheClearable clearable)
        {
            clearable.ClearCache();
        }

        await _dependencyMapper.MapDependenciesCached();

        return 0;
    }

    private async IAsyncEnumerable<DueInstallTask> ValidateAndInsertInternalAsync(
        IEnumerable<QueuedDownloadTask> tasks)
    {
        foreach (var task in tasks)
        {
            var result = await task.Task;
            if (result == DownloadResult.Failed)
            {
                throw new KnownException($"Download for {task.Metadata.Id} ({task.Metadata.Version}) have FAILED.");
            }

            AnsiConsole.MarkupLineInterpolated($"[bold white]:gear:[/] [deepskyblue1] -- {task.Metadata.Id} -- [/]");

            if (task.Stream != null)
            {
                await task.Stream.DisposeAsync();
            }

            var path = task.Objective;
            if (!await _checksumRunner.ValidateAsync(task.ArtefactInfo, path))
            {
                throw new KnownException("Artefact hash mismatches downloaded file.");
            }

            yield return new DueInstallTask(task.Metadata, path, task.IsExplicit);
        }
    }
}
