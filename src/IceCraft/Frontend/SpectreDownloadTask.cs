namespace IceCraft.Frontend;
using System;
using IceCraft.Core.Network;
using Spectre.Console;

public class SpectreDownloadTask : INetworkDownloadTask
{
    private const double Megabyte = 1024 * 1024;
    private const double Kilobyte = 1024;

    private readonly ProgressTask _progressTask;

    internal SpectreDownloadTask(ProgressTask progressTask)
    {
        _progressTask = progressTask;
    }

    private static string GetUserReadableSize(double size)
    {
        if (size >= Megabyte)
        {
            return $"{size / Megabyte:N3} MiB";
        }

        if (size >= Kilobyte)
        {
            return $"{size / Kilobyte:N3} KiB";
        }

        return $"{size:N3} B";
    }

    private static string GetUserReadableSpeed(double bps)
    {
        if (bps >= Megabyte)
        {
            return $"{bps / Megabyte:N3} MiB/s";
        }

        if (bps >= Kilobyte)
        {
            return $"{bps / Kilobyte:N3} KiB/s";
        }

        return $"{bps:N3} B/s";
    }

    public void Complete()
    {
        _progressTask.StopTask();
    }

    public void SetDefinitePrecentage(double precentage)
    {
        _progressTask.IsIndeterminate = false;
        _progressTask.MaxValue = 100;
        _progressTask.Value = precentage;
    }

    public void SetDefiniteProgress(long progress, long max)
    {
        _progressTask.IsIndeterminate = false;
        _progressTask.MaxValue = max;
        _progressTask.Value = progress;
    }

    public void SetIntermediateProgress()
    {
        _progressTask.IsIndeterminate = true;
    }

    public void UpdateSpeed(double bps, long totalBytesToReceive, long receivedBytes)
    {
        _progressTask.Description = $"{GetUserReadableSize(receivedBytes)}/{GetUserReadableSize(totalBytesToReceive)} - {GetUserReadableSpeed(bps)}";
    }
}
