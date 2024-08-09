namespace IceCraft.Core.Network;
using System;

public interface INetworkDownloadTask
{
    void SetDefiniteProgress(long progress, long max);
    void SetIntermediateProgress();
    void Complete();
}
