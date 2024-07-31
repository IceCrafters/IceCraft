namespace IceCraft.Frontend;

using System.ComponentModel;
using Spectre.Console.Cli;

public class BaseSettings : CommandSettings
{
    [CommandOption("--verbose")]
    [Description("Displays trace output.")]
    public bool Verbose { get; set; }
}