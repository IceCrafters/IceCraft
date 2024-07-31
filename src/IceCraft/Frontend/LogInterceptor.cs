namespace IceCraft.Frontend;

using System.Diagnostics;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using Spectre.Console.Cli;

public class LogInterceptor : ICommandInterceptor
{
    void ICommandInterceptor.Intercept(CommandContext context, CommandSettings settings)
    {
        var logSwitch = new LoggingLevelSwitch();

        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console(outputTemplate: "[{Level:u3}] {Message:lj}{NewLine}{Exception}",
                             theme: AnsiConsoleTheme.Code,
                             levelSwitch: logSwitch)
            .CreateLogger();

        if (settings is BaseSettings baseSettings
            && baseSettings.Verbose)
        {
            Log.Verbose("Verbose logging enabled.");
            logSwitch.MinimumLevel = LogEventLevel.Verbose;
        }
    }
}