namespace IceCraft.Core.Installation.Analysis;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using IceCraft.Core.Serialization;
using Semver;

public sealed record DependencyMapEntry
{
    public DependencyMapEntry()
    {
    }

    [SetsRequiredMembers]
    public DependencyMapEntry(string packageName, SemVersion version)
    {
        PackageName = packageName;
        Version = version;

        Dependencies = [];
        Dependents = [];
    }
    
    public required string PackageName { get; init; }
    [JsonConverter(typeof(SemVersionConverter))]
    public required SemVersion Version { get; init; }

    public bool HasUnsatisifiedDependencies { get; set; }

    public required IList<PackageReference> Dependencies { get; init; }

    public required IList<PackageReference> Dependents { get; init; }
}