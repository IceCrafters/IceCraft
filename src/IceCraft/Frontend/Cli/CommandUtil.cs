namespace IceCraft.Frontend.Cli;

using System.CommandLine;
using System.CommandLine.Invocation;
using IceCraft.Core.Util;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

public static class CommandUtil
{
    public static T? GetOpt<T>(this InvocationContext context, Option<T> option)
    {
        return context.ParseResult.GetValueForOption(option);
    }

    public static T GetOptNotNull<T>(this InvocationContext context, Option<T> option)
    {
        var result = context.ParseResult.GetValueForOption(option);

        if (result is null)
        {
            throw new KnownException($"{option.Name} is null");
        }

        return result;
    }

    public static T GetOptNotNull<T>(this InvocationContext context, Option<T?> option)
        where T : struct
    {
        var result = context.ParseResult.GetValueForOption(option);

        if (!result.HasValue)
        {
            throw new KnownException($"{option.Name} is null");
        }

        return result.Value;
    }

    public static T GetArg<T>(this InvocationContext context, Argument<T> argument)
    {
        return context.ParseResult.GetValueForArgument(argument);
    }

    public static T GetArgNotNull<T>(this InvocationContext context, Argument<T> argument)
    {
        var result = context.ParseResult.GetValueForArgument(argument);

        if (result is null)
        {
            throw new KnownException($"{argument.Name} is null");
        }

        return result;
    }

    public static T GetArgNotNull<T>(this InvocationContext context, Argument<T?> argument)
        where T : struct
    {
        var result = context.ParseResult.GetValueForArgument(argument);

        if (!result.HasValue)
        {
            throw new KnownException($"{argument.Name} is null");
        }

        return result.Value;
    }

    public static void ConfigureVerbose(this InvocationContext context)
    {
        var optVerbose = context.ParseResult.GetValueForOption(FrontendUtil.OptVerbose);

        var level = LogEventLevel.Information;

        if (optVerbose)
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
            Output.Shared.Verbose("Verbose logging is enabled");
        }
    }
}
