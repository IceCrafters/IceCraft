namespace IceCraft.Core.Platform.Linux;

using System.Runtime.Versioning;

[SupportedOSPlatform("linux")]
public static class LibCDetector
{
    private const string Ldd = "/usr/bin/ldd";
    private const string GLibCTitle = "GNU C Library";
    private const string MuslTitle = "musl";

    public static CLibraryType DetectCLibrary()
    {
        if (!File.Exists(Ldd))
        {
            // Cannot determine C Library type without ldd for now
            return CLibraryType.Unknown;
        }

        foreach (var line in File.ReadLines(Ldd))
        {
            if (line.Contains(GLibCTitle))
            {
                return CLibraryType.Gnu;
            }

            if (line.Contains(MuslTitle))
            {
                return CLibraryType.Musl;
            }
        }

        return CLibraryType.Unknown;
    }

    public static async Task<CLibraryType> DetectCLibraryAsync()
    {
        if (!File.Exists(Ldd))
        {
            // Cannot determine C Library type without ldd for now
            return CLibraryType.Unknown;
        }

        await foreach (var line in File.ReadLinesAsync(Ldd))
        {
            if (line.Contains(GLibCTitle))
            {
                return CLibraryType.Gnu;
            }

            if (line.Contains(MuslTitle))
            {
                return CLibraryType.Musl;
            }
        }

        return CLibraryType.Unknown;
    }
}
