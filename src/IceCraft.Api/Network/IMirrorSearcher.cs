namespace IceCraft.Api.Network;

using IceCraft.Api.Archive.Artefacts;

public interface IMirrorSearcher
{
    Task<ArtefactMirrorInfo?> GetBestMirrorAsync(IEnumerable<ArtefactMirrorInfo>? mirrors);
}
