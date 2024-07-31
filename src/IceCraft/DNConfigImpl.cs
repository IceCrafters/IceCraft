namespace IceCraft;

using DotNetConfig;
using IceCraft.Core.Configuration;

internal class DNConfigImpl : IManagerConfiguration
{
    private readonly Config _config;

    private const string SectionSources = "source";
    private const string EntrySourcesEnabled = "enabled";

    internal DNConfigImpl(Config config)
    {
        _config = config;
    }

    public bool IsSourceEnabled(string sourceId)
    {
        return _config.GetBoolean(SectionSources, sourceId, EntrySourcesEnabled) ?? false;
    }

    public void SetSourceEnabled(string sourceId, bool enabled)
    {
        _config.SetBoolean(SectionSources, sourceId, EntrySourcesEnabled, enabled);
    }
}
