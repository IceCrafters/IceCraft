// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Frontend.Cli;

using System.CommandLine;

public static class FrontendUtil
{
    internal static readonly Option<bool> OptVerbose = new("--verbose", "Enable verbose output");
    internal static readonly Option<bool> OptDebug = new("--debug", "Allow debugger attach confirmation before acting");
    internal static readonly string BaseName = Path.GetFileName(Environment.GetCommandLineArgs()[0]);
}