namespace IceCraft.Frontend;

using IceCraft.Core.Platform;
using Spectre.Console;

public class SpectreProgressedTask : IProgressedTask
{
    private readonly ProgressTask _progressTask;

    internal SpectreProgressedTask(ProgressTask progressTask)
    {
        _progressTask = progressTask;
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

    public void SetText(string text)
    {
        _progressTask.Description = text;
    }
}
