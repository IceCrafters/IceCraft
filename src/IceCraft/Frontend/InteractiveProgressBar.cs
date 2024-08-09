namespace IceCraft.Frontend;

using IceCraft.Core.Interactivity;
using Spectre.Console;

public class InteractiveProgressBar : IProgressBar
{
    private readonly ProgressTask _progressTask;
    
    public void SetText(string text)
    {
        _progressTask.Description = text;
    }

    public void SetValue(int value)
    {
        _progressTask.Value = value;
    }

    public void Destroy()
    {
        // Cannot do that here
    }
}