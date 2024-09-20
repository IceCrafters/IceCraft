// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft;

using DotNetConfig;
using IceCraft.Api.Client;

internal class DotNetConfigServiceImpl : IManagerConfiguration
{
    private readonly Config _config = Config.Build();
    private readonly IFrontendApp _frontend;

    public DotNetConfigServiceImpl(IFrontendApp frontend)
    {
        _frontend = frontend;
    }

    private const string SectionSources = "source";
    private const string EntrySourcesEnabled = "enabled";

    private const string SectionBehaviour = "behaviour";
    private const string EntryBehaviourAllowUncertainHash = "allowUncertainHash";

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
        return Path.Combine(_frontend.DataBasePath, "caches");
    }
}
