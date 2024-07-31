namespace IceCraft.Frontend;

using System;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

public class TypeRegistrar : ITypeRegistrar
{
    private readonly ServiceCollection _collection;

    private ServiceProvider? _provider;
    public ServiceProvider Provider
    {
        get
        {
            System.Console.WriteLine("Trace: Rolling a provider");

            _provider ??= _collection.BuildServiceProvider();
            return _provider;
        }
    }

    public void Reroll()
    {
        System.Console.WriteLine("Trace: Resetting provider status");
        _provider = null;
    }

    public TypeRegistrar(ServiceCollection collection)
    {
        _collection = collection;
    }

    public ITypeResolver Build()
    {
        return new TypeResolver(Provider);
    }

    public void Register(Type service, Type implementation)
    {
        System.Console.WriteLine("Trace: Registering type {0} as impl of {1}", implementation, service);
        _collection.AddSingleton(service, implementation);
    }

    public void RegisterInstance(Type service, object implementation)
    {
        System.Console.WriteLine("Trace: Registering type {0} as impl of {1}", implementation.GetType(), service);
        _collection.AddSingleton(service, implementation);
    }

    public void RegisterLazy(Type service, Func<object> factory)
    {
        System.Console.WriteLine("Trace: Registering a lazy factory as impl of {0}", service);
        _collection.AddSingleton(service, factory);
    }
}
