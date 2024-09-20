// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Api.Plugin;

public interface IServiceRegistry
{
    void RegisterSingleton<TInterface, TImplementation>()
        where TInterface : class
        where TImplementation : class, TInterface;
    
    void RegisterKeyedSingleton<TInterface, TImplementation>(string? key)
        where TInterface : class
        where TImplementation : class, TInterface;
}