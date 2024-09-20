// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Api.Network;

[Obsolete("Use a generic IProgressedTask instead.")]
public interface INetworkDownloadTask
{
    void SetDefiniteProgress(long progress, long max);
    void SetDefinitePrecentage(double precentage);
    void SetIntermediateProgress();
    void UpdateSpeed(double bps, long totalBytesToReceive, long receivedBytes);
    void Complete();
}
