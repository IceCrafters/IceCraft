namespace IceCraft.Frontend;

using System;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

public class TypeRegistrar : ITypeRegistrar
{
    private readonly ServiceCollection _collection;

    public TypeRegistrar(ServiceCollection collection)
    {
        _collection = collection;
    }

    public ITypeResolver Build()
    {
        return new TypeResolver(_collection.BuildServiceProvider());
    }

    public void Register(Type service, Type implementation)
    {
        _collection.AddSingleton(service, implementation);
    }

    public void RegisterInstance(Type service, object implementation)
    {
        _collection.AddSingleton(service, implementation);
    }

    public void RegisterLazy(Type service, Func<object> factory)
    {
        _collection.AddSingleton(service, factory);
    }
}
