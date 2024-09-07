namespace IceCraft.Frontend;

using System;
using Serilog;
using Spectre.Console.Cli;

public sealed class TypeResolver : ITypeResolver, IDisposable
{
    private readonly IServiceProvider _provider;

    public TypeResolver(IServiceProvider provider)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
    }

    public object? Resolve(Type? type)
    {
        if (type == null)
        {
            return null;
        }

        var retVal = _provider.GetService(type);
        if (retVal == null)
        {
            Log.Warning("Cannot satisfy CLI service: {}", type);
        }

        return retVal;
    }

    public void Dispose()
    {
        if (_provider is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
