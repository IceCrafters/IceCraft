// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Frontend;

using IceCraft.Api.Client;
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
