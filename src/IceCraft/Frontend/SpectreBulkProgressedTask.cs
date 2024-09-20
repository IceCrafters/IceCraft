// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Frontend;

using IceCraft.Api.Client;
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
