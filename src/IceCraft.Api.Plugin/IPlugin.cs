namespace IceCraft.Api.Plugin;

public interface IPlugin
{
    PluginMetadata Metadata { get; }

    void Initialize(IServiceRegistry serviceRegistry);
}