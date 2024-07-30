namespace IceCraft.Core.Archive.Repositories;

using System.Diagnostics;
using IceCraft.Core.Archive.Providers;

public class RepositoryManager
{
    private readonly Dictionary<string, IRepositoryProvider> _providers = [];

    public void RegisterProvider(string id, IRepositoryProvider provider)
    {
        _providers.Add(id, provider);
    }

    public async Task<IEnumerable<IRepository>> GetRepositories()
    {
        var list = new List<IRepository>(_providers.Count);

        foreach (var provider in _providers)
        {
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
