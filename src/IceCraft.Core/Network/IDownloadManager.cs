namespace IceCraft.Core.Network;
using System;

public interface IDownloadManager
{
    Task Download(Uri from, string toFile, INetworkDownloadTask? task = null);
}
