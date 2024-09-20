// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Frontend;

using IceCraft.Api.Client;
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