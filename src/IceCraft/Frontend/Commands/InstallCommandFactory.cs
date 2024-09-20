// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Frontend.Commands;

using System.CommandLine;
using IceCraft.Api.Archive.Artefacts;
using IceCraft.Api.Archive.Indexing;
using IceCraft.Api.Archive.Repositories;
using IceCraft.Api.Client;
using IceCraft.Api.Exceptions;
using IceCraft.Api.Installation;
using IceCraft.Api.Installation.Dependency;
using IceCraft.Api.Network;
using IceCraft.Api.Package;
using IceCraft.Frontend.Cli;
using IceCraft.Interactive;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Semver;
using Spectre.Console;

[UsedImplicitly]
public class InstallCommandFactory : ICommandFactory
{
    private readonly IPackageInstallManager _installManager;
    private readonly IPackageIndexer _indexer;
    private readonly IRepositorySourceManager _sourceManager;
    private readonly IDependencyResolver _dependencyResolver;
    private readonly IFrontendApp _frontend;
    private readonly IArtefactManager _artefactManager;

    private readonly InteractiveInstaller _interactiveInstaller;

    public InstallCommandFactory(IServiceProvider serviceProvider)
    {
        _installManager = serviceProvider.GetRequiredService<IPackageInstallManager>();
        _indexer = serviceProvider.GetRequiredService<IPackageIndexer>();
        _sourceManager = serviceProvider.GetRequiredService<IRepositorySourceManager>();
        _dependencyResolver = serviceProvider.GetRequiredService<IDependencyResolver>();
        _frontend = serviceProvider.GetRequiredService<IFrontendApp>();
        _artefactManager = serviceProvider.GetRequiredService<IArtefactManager>();

        var checksumRunner = serviceProvider.GetRequiredService<IChecksumRunner>();
        var downloadManager = serviceProvider.GetRequiredService<IDownloadManager>();
        var dependencyMapper = serviceProvider.GetRequiredService<IDependencyMapper>();
        _interactiveInstaller = new InteractiveInstaller(downloadManager, _installManager, _artefactManager,
            checksumRunner, dependencyMapper);
    }

    public Command CreateCommand()
    {
        var argPackage = new Argument<string>("package", "The package to install");
        var argVersion = new Argument<string?>("version", () => null, "The version to install. If not specified, selects the latest version");

        var optNoCleanArtefact = new Option<bool>("--no-clean-artefact", "Do not clean previous artefact before proceeding");
        var optPrerelease = new Option<bool>(["-P", "--include-prerelease"], "Include prerelease versions when selecting latest version");

        var command = new Command("install", "Install a package")
        {
            argPackage,
            argVersion,
            optNoCleanArtefact,
            optPrerelease
        };
        
        command.SetHandler(async context =>
        {
            context.ExitCode = await ExecuteInternalAsync(context.GetOpt(optNoCleanArtefact),
                context.GetArgNotNull(argPackage),
                context.GetArg(argVersion),
                context.GetOpt(optPrerelease));
        });

        return command;
    }
    
    private async Task<int> ExecuteInternalAsync(bool noCleanArtefact,
        string packageName,
        string? version,
        bool includePrerelease)
    {
        // Step 0: Clean artefacts
        if (!noCleanArtefact)
        {
            AnsiConsole.Status()
                .Start("Cleaning artefacts", _ => _artefactManager.CleanArtefacts());
        }

        // Step 1: Index
        SemVersion? selectedVersion;
        PackageMeta? meta;
        PackageIndex? index = null;
        HashSet<PackageMeta> allPackagesSet = [];

        await AnsiConsole.Status()
            .StartAsync("Indexing remote packages",
                async ctx =>
                {
                    // STEP: Index remote packages
                    index = await _indexer.IndexAsync(_sourceManager);
                    if (!index.TryGetValue(packageName, out var seriesInfo))
                    {
                        throw new KnownException($"No such package series {packageName}");
                    }

                    // STEP: Select version
                    ctx.Status("Acquiring version information");
                    if (!string.IsNullOrWhiteSpace(version))
                    {
                        // Parse user input, and store in specifiedVersion.
                        // Does not need to be that strict on user input since we all make mistakes.
                        selectedVersion = SemVersion.Parse(version, SemVersionStyles.Any);
                    }
                    else
                    {
                        selectedVersion = await Task.Run(() =>
                            seriesInfo.Versions.GetLatestSemVersion(includePrerelease));
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
        if (!_interactiveInstaller.AskConfirmation(allPackagesSet))
        {
            return 0;
        }

        _frontend.Output.Verbose("Initializing download for {0} packages", allPackagesSet.Count);

        // Step 3: download artefacts

        return await _interactiveInstaller.InstallAsync(allPackagesSet, index!);
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
}