namespace IceCraft.Core.Archive.Repositories;

using System;
using IceCraft.Core.Archive.Providers;
using IceCraft.Core.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public class RepositoryManager : IRepositorySourceManager
{
    private readonly IManagerConfiguration _config;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RepositoryManager> _logger;
    private readonly IRepositoryDefaultsSupplier? _defaults;

    public RepositoryManager(IManagerConfiguration config, 
        IServiceProvider serviceProvider,
        ILogger<RepositoryManager> logger,
        IRepositoryDefaultsSupplier? defaults)
    {
        _config = config;
        _serviceProvider = serviceProvider;
        _logger = logger;
        _defaults = defaults;

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

        var factories = _serviceProvider.GetKeyedServices<IRepositorySourceFactory>(null);
        foreach (var factory in factories)
        {
            var source = factory.CreateRepositorySource(_serviceProvider, out var name);
            RegisterSource(name, source);
        }
    }

    private readonly Dictionary<string, IRepositorySource> _sources = [];

    public int Count => _sources.Count;

    public void RegisterSource(string id, IRepositorySource source)
    {
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

    public async Task<IEnumerable<IRepository>> GetRepositoriesAsync()
    {
        var list = new List<IRepository>(_sources.Count);

        foreach (var provider in _sources)
        {
            if (!_config.IsSourceEnabled(provider.Key))
            {
                continue;
            }

            var repo = await provider.Value.CreateRepositoryAsync();
            if (repo == null)
            {
                continue;
            }

            _logger.LogDebug("RepositoryManager: provider gone through: '{Key}'", provider.Key);
            list.Add(repo);
        }

        return list.AsReadOnly();
    }

    public IEnumerable<IRepositorySource> EnumerateSources()
    {
        return _sources.Values;
    }
}
