namespace IceCraft.Repositories.Adoptium;

using IceCraft.Core.Archive;
using IceCraft.Core.Archive.Providers;

public class AdoptiumRepositoryProvider : IRepositoryProvider
{
    private readonly AdoptiumApiClient _client = new();

    public async Task<IRepository?> CreateRepository()
    {
        var releases = await _client.GetAvailableReleases();
        if (releases == null)
        {
            return null;
        }

        return new AdoptiumRepository(releases, this);
    }

    public async Task<IRepository?> RegenerateRepository()
    {
        // TODO proper caching
        return await CreateRepository();
    }
}
