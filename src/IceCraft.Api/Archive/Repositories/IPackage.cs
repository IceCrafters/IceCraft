namespace IceCraft.Api.Archive.Repositories;

using IceCraft.Api.Archive.Artefacts;
using IceCraft.Api.Package;

public interface IPackage
{
    public IPackageSeries Series { get; }

    public RemoteArtefact GetArtefact();

    PackageMeta GetMeta();

    IEnumerable<ArtefactMirrorInfo>? GetMirrors();
}
