namespace IceCraft.Core.Installation;
using System;

public static class SysPathHelper
{
    public static bool IsPathInSysPath(string path)
    {
        var sysPath = Environment.GetEnvironmentVariable("PATH");
        if (sysPath == null)
        {
            return false;
        }

        return sysPath.Contains(path, StringComparison.Ordinal);
    }
}
