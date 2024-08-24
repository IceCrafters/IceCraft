namespace IceCraft.Core.Installation;
using System;

public static class SysPathHelper
{
    public static bool IsPathInSysPath(string path)
    {
        var sysPath = Environment.GetEnvironmentVariable("PATH");
        return sysPath != null && sysPath.Contains(path, StringComparison.Ordinal);
    }
}
