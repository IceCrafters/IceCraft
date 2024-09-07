namespace IceCraft.Extensions.CentralRepo.Interop;

using Mond;
using Semver;

public static class ScriptConvert
{
    public static MondValue FromSemVersion(SemVersion version)
    {
        var result = MondValue.Object();
        result["Major"] = version.Major;
        result["Minor"] = version.Minor;
        result["Patch"] = version.Patch;
        result["Metadata"] = version.Metadata;
        result["Prerelease"] = version.Prerelease;
        result["ToString"] = MondValue.Function((_, _) => version.ToString());
        return result;
    }
}
