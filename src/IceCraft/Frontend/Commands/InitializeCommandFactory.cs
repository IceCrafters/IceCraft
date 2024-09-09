namespace IceCraft.Frontend.Commands;

using System.CommandLine;
using IceCraft.Core.Platform;
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
            _environmentManager.AddUserGlobalPath(Path.Combine(_frontendApp.DataBasePath, "run"));

            Output.Hint("It is recommended that you REOPEN your terminal (or logout/login in tty).");
        });

        return command;
    }
}