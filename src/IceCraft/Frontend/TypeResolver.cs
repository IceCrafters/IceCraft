namespace IceCraft.Frontend;

using System;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

public class TypeResolver : ITypeResolver
{
    private readonly ServiceProvider _provider;

    public TypeResolver(ServiceProvider provider)
    {
        _provider = provider;
    }

    public object? Resolve(Type? type)
    {
        if (type == null)
        {
            return null;
        }

        return _provider.GetService(type);
    }
}
