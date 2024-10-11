namespace IceCraft.Core.Network;

using System;

public class DownloadMission
{
    private readonly HttpClient _client;

    public DownloadMission(HttpClient client)
    {
        _client = client;
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

    private async Task<DownloadMissionResult> DownloadDeterminateInternalAsync(HttpResponseMessage response,
        Stream objective,
        long length,
        CancellationToken cancellation)
    {
        using var stream = await response.Content.ReadAsStreamAsync(cancellation);
        var buffer = new byte[250].AsMemory();
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
        var buffer = new byte[250].AsMemory();
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

    private static int CalculatePrecentage(int reached, long total)
    {
        return (int)(reached / total * 100);
    }
}
