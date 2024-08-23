namespace IceCraft.Core.Installation;

using IceCraft.Core.Archive.Packaging;
using IceCraft.Core.Installation.Analysis;

public sealed record InstalledPackageInfo
{
    public required PackageMeta Metadata { get; init; }
    public required InstallationState State { get; set; }
    public PackageReference? ProvidedBy { get; init; }
}
