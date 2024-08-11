namespace IceCraft.Core.Archive.Indexing;

using IceCraft.Core.Archive.Artefacts;
using IceCraft.Core.Archive.Packaging;

public record CachedPackageInfo
{
    public required PackageMeta Metadata { get; init; }
    public required RemoteArtefact Artefact { get; init; }
    public IEnumerable<ArtefactMirrorInfo>? Mirrors { get; init; }
}
