// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Frontend.Injection;

using Autofac;
using IceCraft.Api.Installation.Database;

internal class LocalDatabaseAccessImpl : ILocalDatabaseAccess
{
    private readonly IComponentContext _context;

    public LocalDatabaseAccessImpl(IComponentContext context)
    {
        _context = context;
    }

    public ILocalDatabaseMutator GetMutator()
    {
        return _context.Resolve<ILocalDatabaseMutator>();
    }

    public ILocalDatabaseReadHandle GetReadHandle()
    {
        return _context.Resolve<ILocalDatabaseReadHandle>();
    }
}
