namespace IceCraft.Frontend;

using IceCraft.Core.Platform;
using Spectre.Console;

public class SpectreBulkProgressedTask : IBulkProgressedTask
{
    private readonly ProgressContext _context;

    public SpectreBulkProgressedTask(ProgressContext context)
    {
        _context = context;
    }

    public IProgressedTask CreateTask(string description)
    {
        return new SpectreProgressedTask(_context.AddTask(description));
    }
}
