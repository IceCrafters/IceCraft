﻿// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Core.Archive.Repositories;

using System;
using IceCraft.Api.Archive.Repositories;
using IceCraft.Api.Client;
using Microsoft.Extensions.DependencyInjection;

public class RepositoryManager : IRepositorySourceManager
{
    private readonly IManagerConfiguration _config;
    private readonly IServiceProvider _serviceProvider;
    private readonly IRepositoryDefaultsSupplier? _defaults;
    private readonly IOutputAdapter _output;

    public RepositoryManager(IManagerConfiguration config, 
        IServiceProvider serviceProvider,
        IRepositoryDefaultsSupplier? defaults,
        IFrontendApp frontendApp)
    {
        _config = config;
        _serviceProvider = serviceProvider;
        _defaults = defaults;
        _output = frontendApp.Output;

        ApplyDefaults();
    }

    private void ApplyDefaults()
    {
        if (_defaults == null)
        {
            return;
        }

        var defaultSources = _defaults.GetDefaultSources();

        foreach (var defaultSource in defaultSources)
        {
            RegisterSource(defaultSource.Key, defaultSource.Value.Invoke(_serviceProvider));
        }

        var factoryDefaults = 0;
        var factories = _serviceProvider.GetServices<IRepositorySourceFactory>();
        foreach (var factory in factories)
        {
            factoryDefaults++;
            var source = factory.CreateRepositorySource(_serviceProvider, out var name);
            RegisterSource(name, source);
        }

        _output.Verbose("{0} defaults from DI", factoryDefaults);
    }

    private readonly Dictionary<string, IRepositorySource> _sources = [];

    public int Count => _sources.Count;

    public void RegisterSource(string id, IRepositorySource source)
    {
        _output.Verbose("Source {0} registered", id);
        _sources.Add(id, source);
    }

    public void RegisterSourceAsService(string key)
    {
        _sources.Add(key, _serviceProvider.GetRequiredKeyedService<IRepositorySource>(key));
    }

    public bool ContainsSource(string id)
    {
        return _sources.ContainsKey(id);
    }

    public IEnumerable<IRepositorySource> EnumerateSources()
    {
        return _sources.Values;
    }

    public async IAsyncEnumerable<KeyValuePair<string, IRepository>> EnumerateRepositoriesAsync()
    {
        _output.Log("Rolling {0} available repositories", _sources.Count);

        foreach (var provider in _sources)
        {
            if (!_config.IsSourceEnabled(provider.Key))
            {
                continue;
            }

            _output.Tagged("ROLL", provider.Key);

            if (provider.Value is IAsyncRepositorySource asyncSource)
            {
                await foreach (var info in asyncSource.CreateRepositoriesAsync())
                {
                    yield return new KeyValuePair<string, IRepository>(info.Name, info.Repository);
                }
            }
            else
            {
                foreach (var info in provider.Value.CreateRepositories())
                {
                    yield return new KeyValuePair<string, IRepository>(info.Name, info.Repository);
                }
            }
        }
    }
}
