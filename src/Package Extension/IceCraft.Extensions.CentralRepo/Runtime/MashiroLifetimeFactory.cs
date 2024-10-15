// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Extensions.CentralRepo.Runtime;

using Microsoft.Extensions.DependencyInjection;

public class MashiroLifetimeFactory : IMashiroLifetimeFactory
{
    private readonly IServiceScopeFactory _scopeFactory;

    public MashiroLifetimeFactory(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public IMashiroStateLifetime Create()
    {
        var scope = _scopeFactory.CreateScope();
        var lifetime = new MashiroStateLifetime(scope.ServiceProvider.GetRequiredService<MashiroState>(),
            scope);
        return lifetime;
    }
}
