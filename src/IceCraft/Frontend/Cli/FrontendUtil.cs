namespace IceCraft.Frontend.Cli;

using System.CommandLine;

public static class FrontendUtil
{
    internal static readonly Option<bool> OptVerbose = new("--verbose", "Enable verbose output");
    internal static readonly Option<bool> OptDebug = new("--debug", "Allow debugger attach confirmation before acting");
    internal static readonly string BaseName = Environment.GetCommandLineArgs()[0];
}