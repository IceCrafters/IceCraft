namespace IceCraft.Frontend.Commands;

using System.CommandLine;
using IceCraft.Core.Platform;

#if LEGACY_INTERFACE
using JetBrains.Annotations;
using Serilog;
using Spectre.Console.Cli;
using System.ComponentModel;
#endif

using CliCommand = System.CommandLine.Command;

#if LEGACY_INTERFACE
[UsedImplicitly]
[Description("Initializes IceCraft for the current user")]
#endif
public class InitializeCommand
#if LEGACY_INTERFACE
    : Command<BaseSettings>
#endif
{
    private readonly IEnvironmentManager _environmentManager;
    private readonly IFrontendApp _frontendApp;

    public InitializeCommand(IEnvironmentManager environmentManager,
        IFrontendApp frontendApp)
    {
        _environmentManager = environmentManager;
        _frontendApp = frontendApp;
    }

    public CliCommand CreateCli()
    {
        var command = new CliCommand("init", "Initializes IceCraft for the current user");
        command.SetHandler(_ =>
        {
            _environmentManager.AddUserGlobalPath(Path.Combine(_frontendApp.DataBasePath, "run"));

            Output.Hint("It is recommended that you REOPEN your terminal (or logout/login in tty).");
        });

        return command;
    }

#if LEGACY_INTERFACE
    public override int Execute(CommandContext context, BaseSettings settings)
    {
        _environmentManager.AddUserGlobalPath(Path.Combine(_frontendApp.DataBasePath, "run"));

        Log.Information("It is recommended that you REOPEN your terminal (or logout/login in tty).");
        return 0;
    }
#endif
}