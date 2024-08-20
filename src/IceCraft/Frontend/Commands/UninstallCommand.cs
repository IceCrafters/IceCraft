namespace IceCraft.Frontend.Commands;

using System.ComponentModel;
using System.Threading.Tasks;
using IceCraft.Core.Archive.Packaging;
using IceCraft.Core.Installation;
using Semver;
using Serilog;
using Spectre.Console;
using Spectre.Console.Cli;

public class UninstallCommand : AsyncCommand<UninstallCommand.Settings>
{
    private readonly IPackageInstallManager _installManager;

    public UninstallCommand(IPackageInstallManager installManager)
    {
        _installManager = installManager;
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

        PackageMeta? selectedVersion = null;
        if (settings.Version != null)
        {
            var semver = SemVersion.Parse(settings.Version, SemVersionStyles.Any);
            selectedVersion = await _installManager.TryGetMetaAsync(settings.PackageName, semver);

            if (selectedVersion == null)
            {
                Log.Fatal("Version {Version} for package {PackageName} not found", settings.Version, settings.PackageName);
                return -2;
            }
        }
        else
        {
            selectedVersion = await _installManager.GetLatestMetaOrDefaultAsync(settings.PackageName);
        }

        await _installManager.UninstallAsync(selectedVersion);
        return 0;
    }

    public sealed class Settings : BaseSettings
    {
        [CommandArgument(0, "<PACKAGE>")]
        [Description("The package to uninstall")]
        public required string PackageName { get; init; }

        [CommandOption("-v|--version")]
        [Description("The version to uninstall")]
        public string? Version { get; init; }
    }
}
