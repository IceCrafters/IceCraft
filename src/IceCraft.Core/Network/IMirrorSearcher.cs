namespace IceCraft.Core.Network;

using IceCraft.Core.Archive.Artefacts;


public interface IMirrorSearcher
{
    Task<ArtefactMirrorInfo?> GetBestMirrorAsync(IEnumerable<ArtefactMirrorInfo>? mirrors);
}
