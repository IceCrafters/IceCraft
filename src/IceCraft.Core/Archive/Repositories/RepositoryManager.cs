namespace IceCraft.Core.Archive.Repositories;

using System.Diagnostics;
using IceCraft.Core.Archive.Providers;
using IceCraft.Core.Configuration;
using Microsoft.Extensions.DependencyInjection;

public class RepositoryManager
{
    private readonly IManagerConfiguration _config;
    private readonly IServiceProvider _serviceProvider;

    public RepositoryManager(IManagerConfiguration config, IServiceProvider serviceProvider)
    {
        _config = config;
        _serviceProvider = serviceProvider;
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

            var repo = await provider.Value.CreateRepository();
            if (repo == null)
            {
                continue;
            }

            Debug.WriteLine($"RepositoryManager: repository acquried: '{provider.Key}'");
            list.Add(repo);
        }

        return list.AsReadOnly();
    }
}
