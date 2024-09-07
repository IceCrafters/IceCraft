namespace IceCraft.Tests.Helpers;

public class LinuxTheoryAttribute : TheoryAttribute
{
    public LinuxTheoryAttribute()
    {
        if (!OperatingSystem.IsLinux())
        {
            Skip = "Theory is only available on Linux";
        }
    }
}
