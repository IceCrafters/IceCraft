namespace IceCraft.Core.Installation.Analysis;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using IceCraft.Core.Serialization;
using Semver;

public sealed record DependencyMapEntry
{
    [SetsRequiredMembers]
    public DependencyMapEntry(string packageName, SemVersion version)
    {
        PackageName = packageName;
        Version = version;
    }
    
    public required string PackageName { get; init; }
    [JsonConverter(typeof(SemVersionConverter))]
    public required SemVersion Version { get; init; }

    public IList<PackageReference> Dependencies { get; } = [];
}