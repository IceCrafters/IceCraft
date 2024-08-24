namespace IceCraft.Frontend.Commands;

using System.ComponentModel;
using IceCraft.Core.Platform;
using JetBrains.Annotations;
using Serilog;
using Spectre.Console.Cli;

[UsedImplicitly]
[Description("Initializes IceCraft for the current user")]
public class InitializeCommand : Command<BaseSettings>
{
    private readonly IEnvironmentManager _environmentManager;
    private readonly IFrontendApp _frontendApp;

    public InitializeCommand(IEnvironmentManager environmentManager,
        IFrontendApp frontendApp)
    {
        _environmentManager = environmentManager;
        _frontendApp = frontendApp;
    }
    
    public override int Execute(CommandContext context, BaseSettings settings)
    {
        _environmentManager.AddUserGlobalPath(Path.Combine(_frontendApp.DataBasePath, "run"));

        Log.Information("It is recommended that you REOPEN your terminal (or logout/login in tty).");
        return 0;
    }
}