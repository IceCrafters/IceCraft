namespace IceCraft.Frontend.Commands;

using System.Threading.Tasks;
using IceCraft.Core.Archive.Packaging;
using IceCraft.Core.Caching;
using IceCraft.Core.Installation;
using IceCraft.Core.Installation.Analysis;
using IceCraft.Frontend.Cli;
using Semver;
using Serilog;
using Spectre.Console;

#if LEGACY_INTERFACE
using JetBrains.Annotations;
using Spectre.Console.Cli;
using System.ComponentModel;
#else
using System.CommandLine;
#endif

#if LEGACY_INTERFACE
[UsedImplicitly]
public class UninstallCommand : AsyncCommand<UninstallCommand.Settings>
#else
public class CliRemoveCommandFactory : ICommandFactory
#endif
{
    private readonly IPackageInstallManager _installManager;
    private readonly IDependencyMapper _dependencyMapper;

    #if LEGACY_INTERFACE
    public UninstallCommand(IPackageInstallManager installManager,
    #else
    public CliRemoveCommandFactory(IPackageInstallManager installManager,
        IDependencyMapper dependencyMapper)
    #endif
    {
        _installManager = installManager;
        _dependencyMapper = dependencyMapper;
    }

    #if !LEGACY_INTERFACE
    public Command CreateCli()
    {
        var argPackage = new Argument<string>("package", "Package to uninstall");
        var argVersion = new Argument<string?>("version", () => null, "Version to uninstall");
        var optForce = new Option<bool>("--force", "Remove package even if it have dependents present");

        var command = new Command("remove", "Uninstall a package")
        {
            argPackage,
            argVersion,
            optForce
        };
        command.SetHandler(async context => context.ExitCode = await ExecuteInternalAsync(
            context.GetArgNotNull(argPackage),
            context.GetArg(argVersion),
            context.GetOpt(optForce)));

        return command;
    }
    #endif
    
    private async Task<int> ExecuteInternalAsync(string packageName, string? version, bool force)
    {
        if (!await _installManager.IsInstalledAsync(packageName))
        {
            Log.Fatal("Package {PackageName} is not installed", packageName);
            return -1;
        }

        PackageMeta? selectedVersion;
        if (version != null)
        {
            var semver = SemVersion.Parse(version, SemVersionStyles.Any);
            selectedVersion = await _installManager.TryGetMetaAsync(packageName, semver);
        }
        else
        {
            selectedVersion = await _installManager.GetLatestMetaOrDefaultAsync(packageName);
        }

        if (selectedVersion == null)
        {
            Output.Shared.Error("Version {0} for package {1} not found", version, packageName);
            return -2;
        }

        try
        {
            var map = await _dependencyMapper.MapDependenciesCached();
            var branch = map.GetValueOrDefault(selectedVersion.Id);
            if (branch != null)
            {
                if (branch.TryGetValue(selectedVersion.Version.ToString(), out var entry)
                    && entry.Dependents.Count > 0
                    && !force)
                {
                    Output.Shared.Error("{0} packages still depends on '{1}' ({2})",
                        entry.Dependents.Count,
                        selectedVersion.Id,
                        selectedVersion.Version);
                    return ExitCodes.ForceOptionRequired;
                }
            }
        }
        catch (Exception ex)
        {
            Output.Shared.Warning("Dependency map is currently BROKEN");
            Output.Verbose(ex, "Details:");

            if (!force)
            {
                Output.Shared.Warning("Use --force to remove package if you know what you are doing");
                return ExitCodes.ForceOptionRequired;
            }
        }

        AnsiConsole.MarkupLineInterpolated($":star: [blue]{selectedVersion.Id}[/] ([green]{selectedVersion.Version}[/]) will be [invert]UNINSTALLED[/]");
        if (!AnsiConsole.Confirm("Uninstall package?", false))
        {
            return ExitCodes.Cancelled;
        }

        await _installManager.UninstallAsync(selectedVersion);

        await AnsiConsole.Status()
            .StartAsync("Evaluating dependency information",
                async _ =>
                {
                    if (_dependencyMapper is ICacheClearable clearable)
                    {
                        clearable.ClearCache();
                    }
                    await _dependencyMapper.MapDependenciesCached();
                });
        return ExitCodes.Ok;
    }
    
    #if LEGACY_INTERFACE
    public override ValidationResult Validate(CommandContext context, Settings settings)
    {
        if (settings.Version != null && !SemVersion.TryParse(settings.Version, SemVersionStyles.Any, out _))
        {
            return ValidationResult.Error("Invalid semantic version.");
        }

        return base.Validate(context, settings);
    }
    
    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        return await ExecuteInternalAsync(settings.PackageName,
            settings.Version,
            settings.Force);
    }

    [UsedImplicitly]
    public sealed class Settings : BaseSettings
    {
        [CommandArgument(0, "<PACKAGE>")]
        [Description("The package to uninstall")]
        [UsedImplicitly]
        public required string PackageName { get; init; }

        [CommandOption("-v|--version")]
        [Description("The version to uninstall")]
        [UsedImplicitly]
        public string? Version { get; init; }

        [CommandOption("--force")]
        [Description("Uninstall package even if it currently have dependents.s")]
        public bool Force { get; init; }
    }
#endif
}
