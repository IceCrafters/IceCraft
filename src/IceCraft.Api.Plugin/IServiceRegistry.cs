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