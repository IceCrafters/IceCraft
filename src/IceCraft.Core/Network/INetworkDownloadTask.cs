namespace IceCraft.Core.Network;
using System;

public interface INetworkDownloadTask
{
    void SetDefiniteProgress(long progress, long max);
    void SetDefinitePrecentage(double precentage);
    void SetIntermediateProgress();
    void UpdateSpeed(double bps, long totalBytesToReceive, long receivedBytes);
    void Complete();
}
