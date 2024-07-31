namespace IceCraft.Core.Archive.Repositories;

using System.Diagnostics;
using IceCraft.Core.Archive.Providers;
using IceCraft.Core.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public class RepositoryManager : IRepositorySourceManager
{
    private readonly IManagerConfiguration _config;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RepositoryManager> _logger;

    public RepositoryManager(IManagerConfiguration config, 
        IServiceProvider serviceProvider,
        ILogger<RepositoryManager> logger)
    {
        _config = config;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    private readonly Dictionary<string, IRepositorySource> _sources = [];

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

    public async Task<IEnumerable<IRepository>> GetRepositories()
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

            _logger.LogTrace("RepositoryManager: provider gone through: '{}'", provider.Key);
            list.Add(repo);
        }

        return list.AsReadOnly();
    }

    public IEnumerable<IRepositorySource> EnumerateSources()
    {
        return _sources.Values;
    }
}
