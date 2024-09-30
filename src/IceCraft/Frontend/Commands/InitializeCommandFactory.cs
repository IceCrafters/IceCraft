// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Frontend.Commands;

using System.CommandLine;
using IceCraft.Api.Client;
using IceCraft.Api.Platform;
using IceCraft.Frontend.Cli;

public class InitializeCommandFactory : ICommandFactory
{
    private readonly IEnvironmentManager _environmentManager;
    private readonly IFrontendApp _frontendApp;

    public InitializeCommandFactory(IEnvironmentManager environmentManager,
        IFrontendApp frontendApp)
    {
        _environmentManager = environmentManager;
        _frontendApp = frontendApp;
    }

    public Command CreateCommand()
    {
        var command = new Command("init", "Initializes IceCraft for the current user");
        command.SetHandler(_ =>
        {
            _environmentManager.AddPath(Path.Combine(_frontendApp.DataBasePath, "run"),
                EnvironmentTarget.Global);

            Output.Hint("It is recommended that you REOPEN your terminal (or logout/login in tty).");
        });

        return command;
    }
}