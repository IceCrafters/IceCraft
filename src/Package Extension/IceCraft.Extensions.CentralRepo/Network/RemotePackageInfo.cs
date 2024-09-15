namespace IceCraft.Extensions.CentralRepo.Network;

using IceCraft.Api.Archive.Artefacts;
using IceCraft.Api.Package;

public record RemotePackageInfo
{
    public required PackageMeta Metadata { get; init; }
    public required RemoteArtefact Artefact { get; init; }
    public required IList<ArtefactMirrorInfo> Mirrors { get; init; }
}