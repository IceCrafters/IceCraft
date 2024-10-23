// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Plugin;

using System.CommandLine;
using IceCraft.Api.Plugin.Commands;

internal class PluginArgumentImpl<T> : IPluginArgument<T>
{
    public PluginArgumentImpl(Argument<T> argument)
    {
        WrappedArgument = argument;
    }

    internal Argument<T> WrappedArgument { get; }
}
