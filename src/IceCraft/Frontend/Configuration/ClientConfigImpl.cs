// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Frontend.Configuration;

using System.Text.Json;
using IceCraft.Api.Client;

public class ClientConfigImpl : IManagerConfiguration
{
    private readonly string _configFilePath = Path.Combine(
        ClientConfigManager.GetEffectiveConfigHome(),
        "client.jsonc");

    private readonly IFrontendApp _frontend;
    private ClientConfigModel? _data;

    public ClientConfigImpl(IFrontendApp frontend)
    {
        _frontend = frontend;
    }

    private ClientConfigModel GetData()
    {
        return _data ??= ReadData();
    }

    private ClientConfigModel ReadData()
    {
        if (!File.Exists(_configFilePath))
        {
            var result = new ClientConfigModel
            {
                EnabledSources = []
            };
            SaveData(result);
            return result;
        }

        try
        {
            using var stream = File.OpenRead(_configFilePath);
            return JsonSerializer.Deserialize(stream, ClientConfigContext.Default.ClientConfigModel)
                ?? new ClientConfigModel
                {
                    EnabledSources = []
                };
        }
        catch (Exception ex)
        {
            Output.Shared.Warning(ex, "Failed to load client config");
            return new ClientConfigModel
            {
                EnabledSources = []
            };
        }
    }

    private void SaveData(ClientConfigModel data)
    {
        try
        {
            using var stream = new FileStream(_configFilePath, FileMode.Create);
            JsonSerializer.SerializeAsync(stream, data, ClientConfigContext.Default.ClientConfigModel);
            stream.Flush(true);
        }
        catch (Exception ex)
        {
            Output.Shared.Warning(ex, "Failed to save client config");
        }
    }

    public bool DoesAllowUncertainHash
    {
        get => GetData().DoesAllowUncertainHash;
        set
        {
            var data = GetData();
            data.DoesAllowUncertainHash = value;
            SaveData(data);
        }
    }
    
    public bool IsSourceEnabled(string sourceId)
    {
        return GetData().EnabledSources.Contains(sourceId);
    }

    public void SetSourceEnabled(string sourceId, bool enabled)
    {
        var data = GetData();
        
        data.EnabledSources.Add(sourceId);
        SaveData(data);
    }
    
    public string GetCachePath()
    {
        if (Environment.OSVersion.Platform != PlatformID.Unix
            || OperatingSystem.IsMacOS()) return Path.Combine(_frontend.DataBasePath, "caches");
        
        // Use XDG_CACHE_HOME (default: ~/.cache)
        var cacheHome = Environment.GetEnvironmentVariable("XDG_CACHE_HOME")
                        ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".cache");
        return Path.Combine(cacheHome, "IceCraft.d");
    }
}