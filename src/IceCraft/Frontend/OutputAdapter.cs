namespace IceCraft.Frontend;

using IceCraft.Core.Platform;
using JetBrains.Annotations;
using Spectre.Console;

public class OutputAdapter : IOutputAdapter
{
    [StringFormatMethod("format")]
    public void Error(string message)
    {
        AnsiConsole.MarkupLineInterpolated($"[bold red]E:[/] [white]{message}[/]");
    }

    [StringFormatMethod("format")]
    public void Error(string format, params object[] args)
    {
        AnsiConsole.MarkupLineInterpolated($"[bold red]E:[/] [white]{string.Format(format, args)}[/]");
    }

    [StringFormatMethod("format")]
    public void Warning(string message)
    {
        AnsiConsole.MarkupLineInterpolated($"[bold yellow]W:[/] [white]{message}[/]");
    }

    [StringFormatMethod("format")]
    public void Warning(string format, params object[] args)
    {
        AnsiConsole.MarkupLineInterpolated($"[bold yellow]W:[/] [white]{string.Format(format, args)}[/]");
    }
}
