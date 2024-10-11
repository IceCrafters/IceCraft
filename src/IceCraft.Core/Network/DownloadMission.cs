// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Core.Network;

using System;

public class DownloadMission
{
    private readonly HttpClient _client;
    private readonly DownloadMissionConfig _config;

    public DownloadMission(HttpClient client) : this(client, DownloadMissionConfig.Default)
    {
    }

    public DownloadMission(HttpClient client, DownloadMissionConfig missionConfig)
    {
        _client = client;
        _config = missionConfig;
    }

    public delegate void MissionProgressDelegate(DownloadMission mission, DownloadMissionProgress progress);
    public delegate void MissionCompleteDelegate(DownloadMission mission, DownloadMissionResult result);

    public MissionProgressDelegate? ProgressCallback { get; set; }

    public async ValueTask<DownloadMissionResult> DownloadAsync(DownloadTarget target,
        string toFile,
        CancellationToken cancellation = default)
    {
        var response = await _client.GetAsync(target.Link, cancellation);
        if (!response.IsSuccessStatusCode)
        {
            return DownloadMissionResult.CreateWithStatusCode(response.StatusCode);
        }

        var length = response.Content.Headers.ContentLength;
        using var stream = File.Create(toFile);
        if (!length.HasValue)
        {
            if (target.ContentLength > 0)
            {
                return await DownloadDeterminateInternalAsync(response, stream, target.ContentLength, cancellation);
            }

            return await DownloadIndeterminateInternalAsync(response, stream, cancellation);
        }
        else
        {
            return await DownloadDeterminateInternalAsync(response, stream, length.Value, cancellation);
        }
    }

    public async ValueTask<DownloadMissionResult> DownloadAsync(DownloadTarget target,
        Stream toStream,
        CancellationToken cancellation = default)
    {
        var response = await _client.GetAsync(target.Link, cancellation);
        if (!response.IsSuccessStatusCode)
        {
            return DownloadMissionResult.CreateWithStatusCode(response.StatusCode);
        }

        var length = response.Content.Headers.ContentLength;
        if (!length.HasValue)
        {
            if (target.ContentLength > 0)
            {
                return await DownloadDeterminateInternalAsync(response, toStream, target.ContentLength, cancellation);
            }

            return await DownloadIndeterminateInternalAsync(response, toStream, cancellation);
        }
        else
        {
            return await DownloadDeterminateInternalAsync(response, toStream, length.Value, cancellation);
        }
    }

    private async Task<DownloadMissionResult> DownloadDeterminateInternalAsync(HttpResponseMessage response,
        Stream objective,
        long length,
        CancellationToken cancellation)
    {
        using var stream = await response.Content.ReadAsStreamAsync(cancellation);
        var buffer = new byte[CalculateBufferSize(length)].AsMemory();
        var downloaded = 0;

        do
        {
            cancellation.ThrowIfCancellationRequested();

            var bytesRead = await stream.ReadAsync(buffer, cancellation);
            if (bytesRead == 0)
            {
                break;
            }

            await objective.WriteAsync(buffer[..bytesRead], cancellation);
            downloaded += bytesRead;

            ProgressCallback?.Invoke(this, DownloadMissionProgress.Determinate(
                CalculatePrecentage(downloaded, length),
                downloaded));
        } while (true);

        return DownloadMissionResult.CreateSuccess();
    }

    private async ValueTask<DownloadMissionResult> DownloadIndeterminateInternalAsync(HttpResponseMessage response,
        Stream objective,
        CancellationToken cancellation)
    {
        using var stream = await response.Content.ReadAsStreamAsync(cancellation);
        var buffer = new byte[_config.MinBufferSize].AsMemory();
        var downloaded = 0;

        do
        {
            cancellation.ThrowIfCancellationRequested();

            var bytesRead = await stream.ReadAsync(buffer, cancellation);
            if (bytesRead == 0)
            {
                break;
            }

            await objective.WriteAsync(buffer[..bytesRead], cancellation);
            downloaded += bytesRead;
            ProgressCallback?.Invoke(this, DownloadMissionProgress.Indeterminate(downloaded));
        } while (true);

        return DownloadMissionResult.CreateSuccess();
    }

    internal int CalculateBufferSize(long contentLength)
    {
        // Just to prevent people filling zero to this.
        if (contentLength == 0L)
        {
            return _config.MinBufferSize;
        }

        // Securely cast to int. A file exceeding int max value limit has to be rediciously big
        // and most likely result buffer size is waaay over the configured maximum size limit, so
        // maximum size limit is used.
        int smallerLength;
        if (contentLength > int.MaxValue)
        {
            // So we throw numbers larger than int MaxValue away. This doesn't mean we can throw away the size
            // and just return MaxBufferSize.
            smallerLength = int.MaxValue;
        }
        else
        {
            smallerLength = (int)contentLength;
        }

        // Default config is 250 -> 250000
        if (smallerLength < _config.MinBufferSize * 1000)
        {
            return _config.MinBufferSize;
        }

        // Normally, 1000th of a file is enough for a buffer
        var devided = smallerLength / 1000;

        // Bounds check.
        if (devided < _config.MinBufferSize)
        {
            return _config.MinBufferSize;
        }
        else if (devided > _config.MaxBufferSize)
        {
            return _config.MaxBufferSize;
        }

        return devided;
    }

    private static int CalculatePrecentage(int reached, long total)
    {
        return (int)(reached / total * 100);
    }
}
