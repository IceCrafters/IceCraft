namespace IceCraft.Core.Installation;

using IceCraft.Core.Archive.Artefacts;
using IceCraft.Core.Archive.Packaging;

public sealed record InstalledPackageInfo
{
    public required PackageMeta Metadata { get; init; }
    public required InstallationState State { get; init; }
}
