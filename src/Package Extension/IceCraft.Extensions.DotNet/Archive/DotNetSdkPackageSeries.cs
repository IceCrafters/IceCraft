namespace IceCraft.Extensions.DotNet.Archive;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using IceCraft.Core.Archive;
using Microsoft.Deployment.DotNet.Releases;
using Semver;

public class DotNetSdkPackageSeries : IPackageSeries
{
    private readonly Product _product;
    private readonly ReadOnlyCollection<ProductRelease> _releases;

    public DotNetSdkPackageSeries(Product product, ReadOnlyCollection<ProductRelease> releases)
    {
        _product = product;
        _releases = releases;
    }

    public string Name => $"dotnet-{_product.ProductVersion}-sdk";

    public async IAsyncEnumerable<IPackage> EnumeratePackagesAsync()
    {
        var selfRid = await DotNetPlatformUtil.GetDotNetRidAsync();

        foreach (var release in _releases)
        {
            var sdk = release.Sdks.MaxBy(x => x.Version.SdkFeatureBand);
            if (sdk == null)
            {
                continue;
            }

            var releaseFile = release.Files.Where(x => x.Rid == selfRid)
                .First();

            yield return new DotNetSdkPackage(sdk,
                releaseFile,
                this);
        }
    }

    public Task<int> GetExpectedPackageCountAsync()
    {
        return Task.FromResult(_releases.Count);
    }

    public Task<IPackage?> GetLatestAsync()
    {
        return Task.FromResult<IPackage?>(null);
    }

    public Task<SemVersion?> GetLatestVersionIdAsync()
    {
        return Task.FromResult<SemVersion?>(null);
    }
}
