namespace IceCraft.Frontend.Commands;

using System.ComponentModel;
using System.Threading.Tasks;
using IceCraft.Core.Archive.Packaging;
using IceCraft.Core.Caching;
using IceCraft.Core.Installation;
using IceCraft.Core.Installation.Analysis;
using JetBrains.Annotations;
using Semver;
using Serilog;
using SharpCompress;
using Spectre.Console;
using Spectre.Console.Cli;

[UsedImplicitly]
public class UninstallCommand : AsyncCommand<UninstallCommand.Settings>
{
    private readonly IPackageInstallManager _installManager;
    private readonly IDependencyMapper _dependencyMapper;

    public UninstallCommand(IPackageInstallManager installManager,
        IDependencyMapper dependencyMapper)
    {
        _installManager = installManager;
        _dependencyMapper = dependencyMapper;
    }

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
        if (!await _installManager.IsInstalledAsync(settings.PackageName))
        {
            Log.Fatal("Package {PackageName} is not installed", settings.PackageName);
            return -1;
        }

        PackageMeta? selectedVersion;
        if (settings.Version != null)
        {
            var semver = SemVersion.Parse(settings.Version, SemVersionStyles.Any);
            selectedVersion = await _installManager.TryGetMetaAsync(settings.PackageName, semver);
        }
        else
        {
            selectedVersion = await _installManager.GetLatestMetaOrDefaultAsync(settings.PackageName);
        }

        if (selectedVersion == null)
        {
            Output.Shared.Error("Version {Version} for package {PackageName} not found", settings.Version, settings.PackageName);
            return -2;
        }

        try
        {
            var map = await _dependencyMapper.MapDependenciesCached();
            var branch = map.GetValueOrDefault(selectedVersion.Id);
            if (branch != null)
            {
                if (branch.TryGetValue(selectedVersion.Version.ToString(), out var entry)
                    && entry.Dependents?.Count > 0
                    && !settings.Force)
                {
                    Output.Shared.Error("{0} packages still depends on '{1}' ({2})",
                        entry.Dependents.Count,
                        selectedVersion.Id,
                        selectedVersion.Version);
                    return -2;
                }
            }
        }
        catch (Exception ex)
        {
            Output.Shared.Warning("Dependency map is currently BROKEN");
            Output.Verbose(ex, "Details:");

            if (!settings.Force)
            {
                Output.Shared.Warning("Use --force to remove package if you know what you are doing");
                return -100;
            }
        }

        AnsiConsole.MarkupLineInterpolated($":star: [blue]{selectedVersion.Id}[/] ([green]{selectedVersion.Version}[/]) will be [invert]UNINSTALLED[/]");
        if (!AnsiConsole.Confirm("Uninstall package?", false))
        {
            return 0;
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
        return 0;
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
}
