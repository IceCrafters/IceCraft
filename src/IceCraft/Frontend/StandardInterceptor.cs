namespace IceCraft.Frontend;

using System;
using System.Diagnostics;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using Spectre.Console.Cli;

public class StandardInterceptor : ICommandInterceptor
{
    void ICommandInterceptor.Intercept(CommandContext context, CommandSettings settings)
    {
        InterceptLogSettings(settings);
        InterceptRootDir(settings);

#if DEBUG
        if (settings is BaseSettings { Debug: true })
        {
            Debugger.Launch();
        }
#endif
    }

    private static void InterceptRootDir(CommandSettings settings)
    {
        if (settings is not BaseSettings baseSettings)
        {
            return;
        }

        if (!string.IsNullOrEmpty(baseSettings.Root))
        {
            IceCraftApp.UserDataOverride = baseSettings.Root;
        }
    }

    private static void InterceptLogSettings(CommandSettings settings)
    {
        var level = LogEventLevel.Information;

        if (settings is BaseSettings { Verbose: true })
        {
            level = LogEventLevel.Verbose;
            Output.IsVerboseMode = true;
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