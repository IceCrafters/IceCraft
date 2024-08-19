namespace IceCraft.Core.Archive.Indexing;

using System.Threading.Tasks;
using IceCraft.Core.Archive.Artefacts;
using IceCraft.Core.Archive.Repositories;
using IceCraft.Core.Caching;
using IceCraft.Core.Serialization;
using Microsoft.Extensions.Logging;

public class CachedIndexer : IPackageIndexer, ICacheClearable
{
    private static readonly Guid CacheStorageId = new("5ce7b9d1-0aea-4aa1-bb9d-e713c457c632");
    private const string PackageIndexStorage = "pkgIndex_v0_1";
    
    private readonly ICacheStorage _cacheStorage;

    private readonly ILogger<CachedIndexer> _logger;

    public CachedIndexer(ICacheManager cacheManager,
        ILogger<CachedIndexer> logger)
    {
        _cacheStorage = cacheManager.GetStorage(CacheStorageId);
        _logger = logger;
    }

    public async Task<PackageIndex> IndexAsync(IRepositorySourceManager manager,
        CancellationToken? token = null)
    {
        var dict = await _cacheStorage.RollJsonAsync(PackageIndexStorage, 
            async () => await GenerateNewIndex(manager, token),
            IceCraftCoreContext.Default.BasePackageIndex_v0_1);

        return new PackageIndex(dict);
    }

    private async Task<Dictionary<string, CachedPackageSeriesInfo>> GenerateNewIndex(IRepositorySourceManager manager, CancellationToken? token = null)
    {
        var index = new Dictionary<string, CachedPackageSeriesInfo>(manager.Count);
        var repos = await manager.GetRepositoriesAsync();
        var repoCount = 0;
        
        foreach (var repo in repos)
        {
            repoCount++;
            token?.ThrowIfCancellationRequested();

            index.EnsureCapacity(index.Count + repo.GetExpectedSeriesCount());
            var seriesList = repo.EnumerateSeries();

            foreach (var series in seriesList)
            {
                token?.ThrowIfCancellationRequested();
                var expectedCount = await series.GetExpectedPackageCountAsync();

                _logger.LogInformation("Indexing series {Name} with {ExpectedCount} packages", series.Name, expectedCount);

                // Creates this whole dictionary of version information.
                var versions = new Dictionary<string, CachedPackageInfo>(
                    expectedCount);

                var pkgInfos = await series.EnumeratePackagesAsync();
                foreach (var pkg in pkgInfos)
                {
                    token?.ThrowIfCancellationRequested();
                    // Go through everything.
                    var pkgMeta = pkg.GetMeta();
                    _logger.LogTrace("Indexing version {Version}", pkgMeta.Version);

                    RemoteArtefact remoteArtefact;
                    IEnumerable<ArtefactMirrorInfo>? mirrors;

                    token?.ThrowIfCancellationRequested();
                    try
                    {
                        remoteArtefact = pkg.GetArtefact();
                        mirrors = pkg.GetMirrors();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to index package {Id} {Version}", pkgMeta.Id, pkgMeta.Version);
                        continue;
                    }

                    versions.Add(pkgMeta.Version.ToString(), new CachedPackageInfo
                    {
                        Metadata = pkgMeta,
                        Artefact = remoteArtefact,
                        Mirrors = mirrors
                    });
                }

                index.Add(series.Name, new CachedPackageSeriesInfo
                {
                    Name = series.Name,
                    Versions = versions,
                });
            }
        }

        _logger.LogInformation("{RepoCount} repositories indexed", repoCount);

        return index;
    }

    public void ClearCache()
    {
        _cacheStorage.DeleteObject(PackageIndexStorage);
    }
}
