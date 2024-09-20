// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Extensions.CentralRepo.Util;

using System.Runtime.InteropServices;
using System.Runtime.Versioning;

internal static class CommandShell
{
    /// <summary>
    /// Executes a command with the platform's default command interpreter.
    /// </summary>
    /// <param name="command">The command to execute.</param>
    [SupportedOSPlatform("windows")]
    [SupportedOSPlatform("linux")]
    internal static int Execute(string command)
    {
        if (OperatingSystem.IsWindows())
        {
            return WindowsCrt._wsystem(command);
        }

        if (OperatingSystem.IsLinux())
        {
            var status = LibC.system(command);
            var errno = Marshal.GetLastPInvokeError();
            if (status != 0 && errno != 0)
            {
                throw LibC.CreateForFork(errno);
            }

            return status;
        }

        throw new PlatformNotSupportedException();
    }
}