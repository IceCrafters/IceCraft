// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Plugin;

using System.CommandLine.Invocation;
using IceCraft.Api.Plugin.Commands;

internal class PluginCommandContextImpl : IPluginCommandContext
{
    private readonly InvocationContext _context;

    public PluginCommandContextImpl(InvocationContext context)
    {
        _context = context;
    }

    public T? GetArgument<T>(IPluginArgument<T> argument)
    {
        if (argument is not PluginArgumentImpl<T> pluginArg)
        {
            throw new ArgumentException("The argument implementation type is not supported.", nameof(argument));
        }

        return _context.ParseResult.GetValueForArgument(pluginArg.WrappedArgument);
    }

    public T? GetOption<T>(IPluginOption<T> option)
    {
        if (option is not PluginOptionImpl<T> pluginOpt)
        {
            throw new ArgumentException("The option implementation type is not supported.", nameof(option));
        }

        return _context.ParseResult.GetValueForOption(pluginOpt.WrappedOption);
    }
}
