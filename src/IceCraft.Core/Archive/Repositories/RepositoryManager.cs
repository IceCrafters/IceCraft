namespace IceCraft.Core.Archive.Repositories;

using System.Diagnostics;
using IceCraft.Core.Archive.Providers;
using IceCraft.Core.Configuration;
using Microsoft.Extensions.DependencyInjection;

public class RepositoryManager : IRepositorySourceManager
{
    private static int _counter;

    private readonly IManagerConfiguration _config;
    private readonly IServiceProvider _serviceProvider;

    public RepositoryManager(IManagerConfiguration config, IServiceProvider serviceProvider)
    {
        _counter++;
        System.Console.WriteLine("Trace: created #{0}", _counter);
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
        Console.WriteLine("Trace: added source {0} to #{1}", key, _counter);
    }

    public bool ContainsSource(string id)
    {
        Console.WriteLine("Trace: query source existence {0} ({1}) from #{2}", id, _sources.ContainsKey(id), _counter);
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
