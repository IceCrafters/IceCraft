namespace IceCraft.Core.Installation.Analysis;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using IceCraft.Core.Archive.Packaging;
using IceCraft.Core.Serialization;
using Semver;

/// <summary>
/// Points to a package stored in the installation database which full <see cref="PackageMeta"/>
/// can be acquired through relevant methods in <see cref="IPackageInstallManager"/>.
/// </summary>
public readonly record struct PackageReference
{
    [SetsRequiredMembers]
    public PackageReference(string package, SemVersion version)
    {
        PackageId = package;
        PackageVersion = version;
    }
    
    public required string PackageId { get; init; }
    [JsonConverter(typeof(SemVersionConverter))]
    public required SemVersion PackageVersion { get; init; }

    public bool DoesPointTo(PackageMeta meta)
    {
        return PackageId == meta.Id
            && PackageVersion == meta.Version;
    }
}