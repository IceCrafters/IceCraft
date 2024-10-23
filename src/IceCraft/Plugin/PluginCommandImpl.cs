// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Plugin;

using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using IceCraft.Api.Plugin.Commands;

internal class PluginCommandImpl : IPluginCommand
{
    private readonly Command _command;

    public PluginCommandImpl(Command command)
    {
        _command = command;
    }

    public IPluginArgument<T> AddArgument<T>(string name, string? description = null, T? defaultValue = default)
    {
        var argument = new Argument<T>(name, description);
        if (defaultValue != null)
        {
            argument.SetDefaultValue(defaultValue);
        }

        var pluginArg = new PluginArgumentImpl<T>(argument);
        _command.AddArgument(argument);

        return pluginArg;
    }

    public IPluginOption<T> AddOption<T>(string[] names, string? description = null,  T? defaultValue = default)
    {
        var option = new Option<T>(names, description);
        if (defaultValue != null)
        {
            option.SetDefaultValue(defaultValue);
        }

        var pluginOpt = new PluginOptionImpl<T>(option);
        _command.AddOption(option);

        return pluginOpt;
    }

    public void SetHandler(AsyncCommandDelegate handler)
    {
        _command.SetHandler(async context => await HandlerWrapperFuncAsync(context, handler));
    }

    public void SetHandler(CommandDelegate handler)
    {
        _command.SetHandler(context => HandlerWrapperFunc(context, handler));
    }

    private static async Task HandlerWrapperFuncAsync(InvocationContext context, AsyncCommandDelegate handler)
    {
        var pluginContext = new PluginCommandContextImpl(context);
        context.ExitCode = await handler.Invoke(pluginContext);
    }

    private static void HandlerWrapperFunc(InvocationContext context, CommandDelegate handler)
    {
        var pluginContext = new PluginCommandContextImpl(context);
        context.ExitCode = handler.Invoke(pluginContext);
    }

    public IPluginCommand AddSubcommand(string name)
    {
        var command = new Command(name);
        _command.AddCommand(command);
        return new PluginCommandImpl(command);
    }
}
