namespace IceCraft.Tests.Helpers;

public class LinuxFactAttribute : FactAttribute
{
    public LinuxFactAttribute()
    {
        if (!OperatingSystem.IsLinux())
        {
            Skip = "Test is only available on Linux";
        }
    }
}
