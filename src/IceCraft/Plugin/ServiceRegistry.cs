// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Plugin;

using IceCraft.Api.Plugin;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

[Obsolete("Service Registry APIs are obsolete")]
public class ServiceRegistry : IServiceRegistry
{
    private readonly IServiceCollection _serviceCollection;

    public ServiceRegistry(IServiceCollection serviceCollection)
    {
        _serviceCollection = serviceCollection;
    }

    public IServiceRegistry RegisterSingleton<TInterface, TImplementation>()
        where TInterface : class
        where TImplementation : class, TInterface
    {
        _serviceCollection.AddSingleton<TInterface, TImplementation>();
        return this;
    }

    public IServiceRegistry RegisterSingleton<T>() where T : class
    {
        _serviceCollection.AddSingleton<T>();
        return this;
    }

    public IServiceRegistry RegisterKeyedSingleton<TInterface, TImplementation>(string? key)
        where TInterface : class
        where TImplementation : class, TInterface
    {
        _serviceCollection.AddKeyedSingleton<TInterface, TImplementation>(key);
        return this;
    }

    public IServiceRegistry RegisterTransitent<[MeansImplicitUse(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)] T>() where T : class
    {
        _serviceCollection.AddTransient<T>();
        return this;
    }
}