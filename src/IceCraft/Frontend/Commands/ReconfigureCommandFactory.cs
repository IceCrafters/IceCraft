namespace IceCraft.Frontend.Commands;

using System.CommandLine;
using IceCraft.Core.Installation;
using IceCraft.Core.Util;
using IceCraft.Frontend.Cli;
using Microsoft.Extensions.DependencyInjection;

public class ReconfigureCommandFactory : ICommandFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IPackageInstallManager _installManager;

    public ReconfigureCommandFactory(IServiceProvider serviceProvider, IPackageInstallManager installManager)
    {
        _serviceProvider = serviceProvider;
        _installManager = installManager;
    }
    
    public Command CreateCommand()
    {
        var argPackage = new Argument<string>("package", "The package to reconfigure");

        var command = new Command("reconfigure", "Reconfigure an installed package")
        {
            argPackage
        };
        
        command.SetHandler(ExecuteAsync, argPackage);
        return command;
    }

    private async Task ExecuteAsync(string package)
    {
        var meta = await _installManager.GetLatestMetaOrDefaultAsync(package);
        if (meta == null)
        {
            throw new KnownException($"No such package '{package}'");
        }

        var configurator = _serviceProvider.GetKeyedService<IPackageConfigurator>(meta.PluginInfo.ConfiguratorRef);
        if (configurator == null)
        {
            throw new KnownException($"Package '{meta.Id}' ({meta.Version}) does not define a valid configurator.");
        }

        var installDir = await _installManager.GetInstalledPackageDirectoryAsync(meta);
        
        await configurator.UnconfigurePackageAsync(installDir, meta);
        await configurator.ConfigurePackageAsync(installDir, meta);
    }
}