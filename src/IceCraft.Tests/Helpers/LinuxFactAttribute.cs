namespace IceCraft.Tests.Helpers;

using System.Runtime.Versioning;

public class LinuxFactAttribute : FactAttribute
{
    public LinuxFactAttribute()
    {
        if (!OperatingSystem.IsLinux())
        {
            Skip = "Fact is only available on Linux";
        }
    }
}
