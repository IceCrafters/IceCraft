namespace IceCraft.Core.Archive.Dependency;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using IceCraft.Core.Serialization;
using Semver;

public readonly record struct DependencyReference
{
    public DependencyReference()
    {
    }

    [SetsRequiredMembers]
    public DependencyReference(string packageId, SemVersionRange versionRange)
    {
        PackageId = packageId;
        VersionRange = versionRange;
    }
    
    public required string PackageId { get; init; }
    
    [JsonConverter(typeof(SemVersionRangeConverter))]
    public required SemVersionRange VersionRange { get; init; }
}