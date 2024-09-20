// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Extensions.DotNet;

using System.Threading.Tasks;
using IceCraft.Api.Archive.Repositories;
using IceCraft.Core.Archive.Repositories;
using IceCraft.Extensions.DotNet.Archive;
using Microsoft.Deployment.DotNet.Releases;

public class DotNetRepositorySource : IRepositorySource
{
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
