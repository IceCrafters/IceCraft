namespace IceCraft.Extensions.DotNet;

using System.Runtime.InteropServices;
using IceCraft.Core.Platform.Linux;

public static class DotNetPlatformUtil
{
    private static readonly HashSet<Architecture> Architectures =
    [
        Architecture.X64,
        Architecture.Arm64
    ];

    public static string GetDotNetRid()
    {
        var system = GetDotNetRidSystemAsync();
        var arch = GetDotNetRidArchitecture(RuntimeInformation.OSArchitecture);

        return $"{system}-{arch}";
    }

    public static async Task<string> GetDotNetRidAsync()
    {
        var system = await GetDotNetRidSystemAsync();
        var arch = GetDotNetRidArchitecture(RuntimeInformation.OSArchitecture);

        return $"{system}-{arch}";
    }

    public static string GetDotNetRidArchitecture(Architecture architecture)
    {
        return architecture switch
        {
            Architecture.Arm64 => "arm64",
            Architecture.X64 => "x64",
            _ => throw new PlatformNotSupportedException()
        };
    }

    public static string GetDotNetRidSystem()
    {
        if (OperatingSystem.IsLinux())
        {
            if (LibCDetector.DetectCLibrary() == CLibraryType.Musl)
            {
                return "linux-musl";
            }

            return "linux";
        }

        return GetNonLinuxRidInternal();
    }

    private static string GetNonLinuxRidInternal()
    {
        if (OperatingSystem.IsWindows())
        {
            return "win";
        }

        if (OperatingSystem.IsMacOS())
        {
            return "osx";
        }

        throw new PlatformNotSupportedException();
    }

    public static async Task<string> GetDotNetRidSystemAsync()
    {
        if (OperatingSystem.IsLinux())
        {
            if (await LibCDetector.DetectCLibraryAsync() == CLibraryType.Musl)
            {
                return "linux-musl";
            }

            return "linux";
        }

        return GetNonLinuxRidInternal();
    }

    public static bool IsArchitectureSupported(Architecture architecture)
    {
        return Architectures.Contains(architecture);
    }
}
