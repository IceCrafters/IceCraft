// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Frontend;

using DotNetConfig;
using IceCraft.Api.Client;

public class CustomConfigImpl : ICustomConfig
{
    private readonly Config _config;

    public CustomConfigImpl(Config config)
    {
        _config = config;
    }
    
    public IConfigScope GetScope(string name)
    {
        return new Scope(_config.GetSection($"plugins.{name}"));
    }
    
    private class Scope : IConfigScope
    {
        private readonly ConfigSection _section;

        internal Scope(ConfigSection section)
        {
            _section = section;
        }
        
        public string? Get(string key)
        {
            return _section.GetString(key);
        }

        public void Set(string key, string value)
        {
            _section.SetString(key, value);
        }

        public void Remove(string key)
        {
            _section.Unset(key);
        }
    }
}