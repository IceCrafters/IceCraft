// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Frontend.Commands;

using System.Threading.Tasks;
using IceCraft.Frontend.Cli;
using Semver;
using Spectre.Console;
using System.CommandLine;
using IceCraft.Api.Caching;
using IceCraft.Api.Installation;
using IceCraft.Api.Installation.Dependency;
using IceCraft.Api.Package;
using Microsoft.Extensions.DependencyInjection;

public class CliRemoveCommandFactory : ICommandFactory
{
    private readonly IPackageInstallManager _installManager;
    private readonly IDependencyMapper _dependencyMapper;
    private readonly IServiceProvider _serviceProvider;

    public CliRemoveCommandFactory(IPackageInstallManager installManager,
        IDependencyMapper dependencyMapper,
        IServiceProvider serviceProvider)
    {
        _installManager = installManager;
        _dependencyMapper = dependencyMapper;
        _serviceProvider = serviceProvider;
    }

    public Command CreateCommand()
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
    
    private async Task<int> ExecuteInternalAsync(string packageName, string? version, bool force)
    {
        if (!_installManager.IsInstalled(packageName))
        {
            Output.Shared.Error("package not installed: {0}", packageName);
            return ExitCodes.PackageNotFound;
        }

        PackageMeta? selectedVersion;
        if (version != null)
        {
            var semver = SemVersion.Parse(version, SemVersionStyles.Any);
            selectedVersion = _installManager.GetMetaOrDefault(packageName, semver);
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

        var agent = _serviceProvider.GetRequiredService<IPackageSetupAgent>();
        await agent.UninstallAsync(selectedVersion);

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
}
