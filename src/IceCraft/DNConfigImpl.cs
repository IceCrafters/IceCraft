﻿namespace IceCraft;

using DotNetConfig;
using IceCraft.Core.Configuration;

internal class DNConfigImpl : IManagerConfiguration
{
    private readonly Config _config;

    private const string SectionSources = "source";
    private const string EntrySourcesEnabled = "enabled";

    private const string SectionBehaviour = "behaviour";
    private const string EntryBehaviourAllowUncertainHash = "allowUncertainHash";

    public DNConfigImpl()
    {
        _config = Config.Build();
    }

    public bool IsSourceEnabled(string sourceId)
    {
        return _config.GetBoolean(SectionSources, sourceId, EntrySourcesEnabled) ?? false;
    }

    public void SetSourceEnabled(string sourceId, bool enabled)
    {
        _config.SetBoolean(SectionSources, sourceId, EntrySourcesEnabled, enabled);
    }

    public bool DoesAllowUncertainHash
    {
        get => _config.GetBoolean(SectionBehaviour, EntryBehaviourAllowUncertainHash) ?? false;
        set => _config.SetBoolean(SectionBehaviour, EntryBehaviourAllowUncertainHash, value);
    }

    public string GetCachePath()
    {
        return IceCraftApp.CachesDirectory;
    }
}
