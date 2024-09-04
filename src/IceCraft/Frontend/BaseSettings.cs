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

    [CommandOption("--root")]
    [Description("Sets the directory where caches, artefacts and installed packages are stored")]
    public string? Root { get; set; }
    
    #if DEBUG
    [CommandOption("--debug")]
    [Description("Wait for a debugger to attach before doing anythinge else.")]
    public bool Debug { get; set; }
    #endif
}