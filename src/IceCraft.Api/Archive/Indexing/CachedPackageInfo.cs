namespace IceCraft.Api.Archive.Indexing;

using IceCraft.Api.Archive.Artefacts;
using IceCraft.Api.Package;

public record CachedPackageInfo
{
    public required PackageMeta Metadata { get; init; }
    public required RemoteArtefact Artefact { get; init; }
    public IEnumerable<ArtefactMirrorInfo>? Mirrors { get; init; }
    public string? BestMirror { get; init; }
}
