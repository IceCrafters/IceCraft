namespace IceCraft.Frontend;

using System.Diagnostics;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using Spectre.Console.Cli;

public class StandardInterceptor : ICommandInterceptor
{
    void ICommandInterceptor.Intercept(CommandContext context, CommandSettings settings)
    {
        var level = LogEventLevel.Information;

        if (settings is BaseSettings { Verbose: true })
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

#if DEBUG
        if (settings is BaseSettings { Debug: true })
        {
            Debugger.Launch();
        }
#endif
    }
}