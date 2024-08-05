namespace IceCraft.Frontend;

using System.ComponentModel;
using JetBrains.Annotations;
using Spectre.Console.Cli;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class BaseSettings : CommandSettings
{
    [CommandOption("--verbose")]
    [Description("Displays trace output.")]
    public bool Verbose { get; set; }
}