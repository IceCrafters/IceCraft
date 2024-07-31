namespace IceCraft.Core.Archive.Repositories;

using System.Diagnostics;
using IceCraft.Core.Archive.Providers;
using IceCraft.Core.Configuration;

public class RepositoryManager
{
    private readonly IManagerConfiguration _config;

    public RepositoryManager(IManagerConfiguration config)
    {
        _config = config;
    }

    private readonly Dictionary<string, IRepositorySource> _providers = [];

    public void RegisterProvider(string id, IRepositorySource provider)
    {
        _providers.Add(id, provider);
    }

    public async Task<IEnumerable<IRepository>> GetRepositories()
    {
        var list = new List<IRepository>(_providers.Count);

        foreach (var provider in _providers)
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
