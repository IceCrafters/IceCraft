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
        }

        throw new PlatformNotSupportedException();
    }
}