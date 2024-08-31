namespace IceCraft.Extensions.CentralRepo.Api;

using Mond.Binding;
using Semver;

[MondClass("PackageVersion")]
public partial class ScrVersion
{
    private readonly SemVersion _semVersion;

    internal ScrVersion(SemVersion version)
    {
        _semVersion = version;
    }
    
    [MondFunction]
    public int Major => _semVersion.Major;
    
    [MondFunction]
    public int Minor => _semVersion.Minor;
    
    [MondFunction]
    public int Patch => _semVersion.Patch;

    [MondFunction]
    public bool IsPrerelease => _semVersion.IsPrerelease;

    [MondFunction]
    public bool IsRelease => _semVersion.IsRelease;
}