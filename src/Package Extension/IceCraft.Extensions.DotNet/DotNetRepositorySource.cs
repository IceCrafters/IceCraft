namespace IceCraft.Extensions.DotNet;

using System.Threading.Tasks;
using IceCraft.Core.Archive;
using IceCraft.Core.Archive.Providers;
using IceCraft.Core.Archive.Repositories;
using IceCraft.Extensions.DotNet.Archive;
using Microsoft.Deployment.DotNet.Releases;

public class DotNetRepositorySource : IRepositorySource
{
    public DotNetRepositorySource()
    {
    }

    public async Task<IRepository?> CreateRepositoryAsync()
    {
        var selfRid = await DotNetPlatformUtil.GetDotNetRidAsync();
        var products = await ProductCollection.GetAsync();
        var repository = new DictionaryRepository(products.Count);
        foreach (var product in products)
        {
            // TODO a deprecated flag in package metadata
            if (!product.IsSupported)
            {
                continue;
            }

            var releases = await product.GetReleasesAsync();
            var sdkSeries = new DotNetSdkPackageSeries(product, releases);

            repository.Add(sdkSeries.Name, sdkSeries);
        }

        return repository;
    }

    public Task RefreshAsync()
    {
        // Nothing to refresh
        return Task.CompletedTask;
    }
}
