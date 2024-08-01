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
        var level = LogEventLevel.Information;

        if (settings is BaseSettings baseSettings
            && baseSettings.Verbose)
        {
            level = LogEventLevel.Verbose;
        }

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Is(level)
            .WriteTo.Console(outputTemplate: "[{Level:u3}] {Message:lj}{NewLine}{Exception}",
                     theme: AnsiConsoleTheme.Code,
                     restrictedToMinimumLevel: level)
            .CreateLogger();

        if (level == LogEventLevel.Verbose)
        {
            Log.Verbose("Verbose logging is enabled.");
        }
    }
}