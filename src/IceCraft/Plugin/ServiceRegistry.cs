namespace IceCraft.Plugin;

using IceCraft.Api.Plugin;
using Microsoft.Extensions.DependencyInjection;

public class ServiceRegistry : IServiceRegistry
{
    private readonly IServiceCollection _serviceCollection;

    public ServiceRegistry(IServiceCollection serviceCollection)
    {
        _serviceCollection = serviceCollection;
    }

    public void RegisterSingleton<TInterface, TImplementation>()
        where TInterface : class
        where TImplementation : class, TInterface
    {
        _serviceCollection.AddSingleton<TInterface, TImplementation>();
    }

    public void RegisterKeyedSingleton<TInterface, TImplementation>(string? key)
        where TInterface : class
        where TImplementation : class, TInterface
    {
        _serviceCollection.AddKeyedSingleton<TInterface, TImplementation>(key);
    }
}