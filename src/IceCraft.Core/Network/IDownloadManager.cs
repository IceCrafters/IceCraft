namespace IceCraft.Core.Network;
using System;
using IceCraft.Core.Archive.Artefacts;
using IceCraft.Core.Archive.Indexing;

public interface IDownloadManager
{
    Task DownloadAsync(Uri from, string toFile, INetworkDownloadTask? downloadTask = null);
    Task DownloadAsync(Uri from, Stream toStream, INetworkDownloadTask? downloadTask = null);
    Task DownloadAsync(ArtefactMirrorInfo fromMirror, Stream toStream, INetworkDownloadTask? downloadTask = null);

    Task<string> DownloadTemporaryArtefactAsync(CachedPackageInfo packageInfo);
    Task<string> DownloadTemporaryArtefactAsync(ArtefactMirrorInfo mirror);
}
