// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Extensions.CentralRepo.Runtime;

using Microsoft.Extensions.DependencyInjection;

public class MashiroStateLifetime : IMashiroStateLifetime, IDisposable
{
    private readonly IDisposable _scope;
    private bool disposedValue;

    public MashiroStateLifetime(MashiroState state, IDisposable scope)
    {
        State = state;
        _scope = scope;
    }

    public MashiroState State
    {
        get;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _scope.Dispose();
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
