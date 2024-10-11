// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Core.Network;

public readonly record struct DownloadMissionConfig
{
    public DownloadMissionConfig()
    {
    }

    public static DownloadMissionConfig Default { get; } = new DownloadMissionConfig
    {
        MinBufferSize = 250,
        MaxBufferSize = 1024
    };

    /// <summary>
    /// Gets the minimum size of memory in bytes allocated to the buffer (and the amount of bytes to
    /// retrieve each loop).
    /// </summary>
    /// <remarks>
    /// <para>
    /// The actual buffer size used is dependent on the expected content length of the file that is downloaded. Whenever
    /// a file downloaded has a size in bytes that is 1000x larger than the value of this property, the buffer size is 
    /// increased automatically. This buffer size will not be smaller than <see cref="MinBufferSize"/> and will not
    /// be larger than <see cref="MaxBufferSize"/>.
    /// </para>
    /// </remarks>
    public required int MinBufferSize { get; init; }

    /// <summary>
    /// Gets the maximum size of memory in bytes allocated to the buffer (and the amount of bytes to
    /// retrieve each loop).
    /// </summary>
    /// <remarks>
    /// <para>
    /// The actual buffer size used is dependent on the expected content length of the file that is downloaded. Whenever
    /// a file downloaded has a size in bytes that is 1000x larger than <see cref="MinBufferSize"/>, the buffer size is 
    /// increased automatically. This buffer size will not be smaller than <see cref="MinBufferSize"/> and will not
    /// be larger than <see cref="MaxBufferSize"/>.
    /// </para>
    /// </remarks>
    public required int MaxBufferSize { get; init; }
}
