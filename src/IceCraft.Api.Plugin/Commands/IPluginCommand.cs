// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Api.Plugin.Commands;

public delegate Task<int> AsyncCommandDelegate(IPluginCommandContext context);
public delegate int CommandDelegate(IPluginCommandContext context);

/// <summary>
/// Defines a plugin command.
/// </summary>
public interface IPluginCommand
{
    IPluginArgument<T> AddArgument<T>(string name, string? description = null, T? defaultValue = default);
    IPluginOption<T> AddOption<T>(string[] names, string? description = null, T? defaultValue = default);
    IPluginCommand AddSubcommand(string name);

    void SetHandler(AsyncCommandDelegate handler);
    void SetHandler(CommandDelegate handler);
}
