namespace IceCraft.Tests.CentralRepo;

public sealed class LinuxFactAttribute : FactAttribute
{
    public LinuxFactAttribute()
    {
        if (!OperatingSystem.IsLinux())
        {
            Skip = "Fact is only available on Linux";
        }
    }
}
