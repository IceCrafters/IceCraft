// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Frontend.Commands;

using System.CommandLine;
using IceCraft.Api.Exceptions;
using IceCraft.Api.Installation;
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
        var meta = await _installManager.GetLatestMetaOrDefaultAsync(package)
            ?? throw new KnownException($"No such package '{package}'");
            
        var agent = _serviceProvider.GetRequiredService<IPackageSetupAgent>();
        await agent.ReconfigureAsync(meta);
    }
}