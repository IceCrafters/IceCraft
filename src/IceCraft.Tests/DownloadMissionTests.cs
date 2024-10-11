// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Tests;

using IceCraft.Core.Network;

public class DownloadMissionTests
{
    [Fact]
    public void CalculateBufferSize_SmallBuffer()
    {
        // Arrange
        const int BufferSize = 250;

        using var client = new HttpClient();
        var mission = new DownloadMission(client, DownloadMissionConfig.Default with
        {
            MinBufferSize = BufferSize
        });

        // Act
        // 52000 / 1000 = 52
        var actualSize = mission.CalculateBufferSize(52000);

        // Assert
        Assert.Equal(BufferSize, actualSize);
    }

    [Fact]
    public void CalculateBufferSize_NormalBuffer()
    {
        // Arrange
        const int MinBufferSize = 250;

        using var client = new HttpClient();
        var mission = new DownloadMission(client, DownloadMissionConfig.Default with
        {
            MinBufferSize = MinBufferSize
        });

        // Act
        // 820 * 1000 = 820000
        var actualSize = mission.CalculateBufferSize(820000);

        // Assert
        Assert.Equal(820, actualSize);
    }

    [Fact]
    public void CalculateBufferSize_LargeBuffer()
    {
        // Arrange
        const int MaxBufferSize = 1024;

        using var client = new HttpClient();
        var mission = new DownloadMission(client, DownloadMissionConfig.Default with
        {
            MaxBufferSize = MaxBufferSize
        });

        // Act
        // 2048 * 1000 = 2048000
        var actualSize = mission.CalculateBufferSize(2048000);

        // Assert
        Assert.Equal(MaxBufferSize, actualSize);
    }
}
