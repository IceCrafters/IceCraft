namespace IceCraft.Api.Installation;

using IceCraft.Api.Installation.Dependency;
using IceCraft.Api.Package;

public sealed record InstalledPackageInfo
{
    public required PackageMeta Metadata { get; init; }
    public required InstallationState State { get; set; }
    public PackageReference? ProvidedBy { get; init; }
}
