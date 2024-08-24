namespace IceCraft.Frontend.Commands;

using System.ComponentModel;
using IceCraft.Core.Installation;
using IceCraft.Core.Util;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

[Description("Reconfigures an already installed package.")]
[UsedImplicitly]
public class PackageReconfigureCommand : AsyncCommand<PackageReconfigureCommand.Settings>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IPackageInstallManager _installManager;

    public PackageReconfigureCommand(IServiceProvider serviceProvider, IPackageInstallManager installManager)
    {
        _serviceProvider = serviceProvider;
        _installManager = installManager;
    }
    
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public sealed class Settings : BaseSettings
    {
        [CommandArgument(0, "<PACKAGE>")]
        [Description("The package to reconfigure.")]
        public required string PackageName { get; init; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var meta = await _installManager.GetLatestMetaOrDefaultAsync(settings.PackageName);
        if (meta == null)
        {
            throw new KnownException($"No such package '{settings.PackageName}'");
        }

        var configurator = _serviceProvider.GetKeyedService<IPackageConfigurator>(meta.PluginInfo.ConfiguratorRef);
        if (configurator == null)
        {
            throw new KnownException($"Package '{meta.Id}' ({meta.Version}) does not define a valid configurator.");
        }

        var installDir = await _installManager.GetInstalledPackageDirectoryAsync(meta);
        
        await configurator.UnconfigurePackageAsync(installDir, meta);
        await configurator.ConfigurePackageAsync(installDir, meta);

        return 0;
    }
}