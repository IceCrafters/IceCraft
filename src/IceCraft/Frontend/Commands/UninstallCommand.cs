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
            Log.Fatal("Version {Version} for package {PackageName} not found", settings.Version, settings.PackageName);
            return -2;
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
    }
}
