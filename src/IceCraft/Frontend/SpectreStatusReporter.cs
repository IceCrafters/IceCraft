namespace IceCraft.Frontend;

using IceCraft.Core.Platform;
using Spectre.Console;

public sealed class SpectreStatusReporter : IStatusReporter
{
    private readonly StatusContext _context;

    public SpectreStatusReporter(StatusContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }
    
    public void UpdateStatus(string status)
    {
        _context.Status(status);
    }
}