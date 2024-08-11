namespace IceCraft.Core.Archive;

using IceCraft.Core.Archive.Artefacts;
using IceCraft.Core.Archive.Packaging;

public interface IPackage
{
    public IPackageSeries Series { get; }

    public RemoteArtefact GetArtefact();

    PackageMeta GetMeta();

    IEnumerable<ArtefactMirrorInfo>? GetMirrors();
}
