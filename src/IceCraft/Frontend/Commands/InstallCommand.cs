namespace IceCraft.Frontend.Commands;

using System.CommandLine;
using IceCraft.Core.Archive.Artefacts;
using IceCraft.Core.Archive.Checksums;
using IceCraft.Core.Archive.Dependency;
using IceCraft.Core.Archive.Indexing;
using IceCraft.Core.Archive.Packaging;
using IceCraft.Core.Archive.Repositories;
using IceCraft.Core.Installation;
using IceCraft.Core.Installation.Analysis;
using IceCraft.Core.Network;
using IceCraft.Core.Platform;
using IceCraft.Core.Util;
using IceCraft.Frontend.Cli;
using IceCraft.Interactive;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Semver;
using Spectre.Console;

#if LEGACY_INTERFACE
using System.ComponentModel;
using Spectre.Console.Cli;
#endif

using CliCommand = System.CommandLine.Command;

[UsedImplicitly]
public class InstallCommand
#if LEGACY_INTERFACE
    : AsyncCommand<InstallCommand.Settings>
#endif
{
    private readonly IPackageInstallManager _installManager;
    private readonly IPackageIndexer _indexer;
    private readonly IRepositorySourceManager _sourceManager;
    private readonly IDependencyResolver _dependencyResolver;
    private readonly IFrontendApp _frontend;
    private readonly IArtefactManager _artefactManager;

    private readonly InteractiveInstaller _interactiveInstaller;

    public InstallCommand(IServiceProvider serviceProvider)
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

    public CliCommand CreateCli()
    {
        var argPackage = new Argument<string>("package", "The package to install");
        var argVersion = new Argument<string?>("version", () => null, "The version to install. If not specified, selects the latest version");

        var optNoCleanArtefact = new Option<bool>("--no-clean-artefact", "Do not clean previous artefact before proceeding");
        var optPrerelease = new Option<bool>(["-P", "--include-prerelease"], "Include prerelease versions when selecting latest version");

        var command = new CliCommand("install", "Install a package")
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

    #if LEGACY_INTERFACE
    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        return await ExecuteInternalAsync(settings.NoCleanArtefact,
            settings.PackageName,
            settings.Version,
            settings.IncludePrerelease);
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
        [UsedImplicitly]
        public bool NoCleanArtefact { get; init; }
    }
#endif
}