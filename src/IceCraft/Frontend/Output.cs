// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Frontend;

using System;
using IceCraft.Api.Client;
using JetBrains.Annotations;
using Spectre.Console;

public class Output : IOutputAdapter
{
    internal static bool IsVerboseMode { get; set; }

    internal static Output Shared { get; } = new();

    public static void Tagged(string tag, string message)
    {
        AnsiConsole.MarkupLineInterpolated($"[bold seagreen1]{tag}:[/] [white]{message}[/]");
    }

    public static void Verbose(string message)
    {
        if (!IsVerboseMode)
        {
            return;
        }

        AnsiConsole.MarkupLineInterpolated($"[bold white]V:[/] [gray]{message}[/]");
    }

    public static void Verbose(Exception exception, string message)
    {
        if (!IsVerboseMode)
        {
            return;
        }
        
        Verbose(message);
        AnsiConsole.WriteException(exception);
    }

    public static void Hint(string message)
    {
        AnsiConsole.MarkupLineInterpolated($"[bold yellow]HINT:[/] [white]{message}[/]");
    }

    #region Tagged

    [StringFormatMethod("format")]
    public static void Tagged(string tag, string format, params object?[] args)
    {
        Tagged(tag, string.Format(format, args));
    }

    void IOutputAdapter.Tagged(string tag, string message)
    {
        Tagged(tag, message);
    }

    void IOutputAdapter.Tagged(string tag, string format, params object?[] args)
    {
        Tagged(tag, format, args);
    }

    public void Error(string message)
    {
        AnsiConsole.MarkupLineInterpolated($"[bold red]E:[/] [white]{message}[/]");
    }

    [StringFormatMethod("format")]
    public void Error(string format, params object?[] args)
    {
        Error(string.Format(format, args));
    }

    #endregion

    public void Log(string message)
    {
        AnsiConsole.MarkupLineInterpolated($"[grey74]{message}[/]");
    }

    [StringFormatMethod("format")]
    public void Log(string format, params object?[] args)
    {
        Log(string.Format(format, args));
    }
    public void Warning(string message)
    {
        AnsiConsole.MarkupLineInterpolated($"[bold yellow]W:[/] [white]{message}[/]");
    }

    [StringFormatMethod("format")]
    public void Warning(string format, params object?[] args)
    {
        Warning(string.Format(format, args));
    }

    public void Warning(Exception exception, string message)
    {
        Warning(message);
        AnsiConsole.WriteException(exception);
    }

    [StringFormatMethod("format")]
    public void Verbose(string format, params object?[] args)
    {
        if (!IsVerboseMode)
        {
            return;
        }

        Verbose(string.Format(format, args));
    }

    void IOutputAdapter.Verbose(string message)
    {
        Verbose(message);
    }
}
