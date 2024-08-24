namespace IceCraft.Extensions.DotNet.Archive;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using IceCraft.Core.Archive;
using IceCraft.Core.Archive.Repositories;
using Microsoft.Deployment.DotNet.Releases;
using Semver;

public class DotNetSdkPackageSeries : AsyncPackageSeries
{
    private readonly Product _product;
    private readonly ReadOnlyCollection<ProductRelease> _releases;

    public DotNetSdkPackageSeries(Product product, ReadOnlyCollection<ProductRelease> releases)
    {
        _product = product;
        _releases = releases;
    }

    public override string Name => $"dotnet-{_product.ProductVersion}-sdk";

    public override async IAsyncEnumerable<IPackage> EnumeratePackagesAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var selfRid = await DotNetPlatformUtil.GetDotNetRidAsync();

        foreach (var release in _releases)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var sdk = release.Sdks.MaxBy(x => x.Version.SdkFeatureBand);
            if (sdk == null)
            {
                continue;
            }

            var releaseFile = release.Files
                .First(x => x.Rid == selfRid);

            yield return new DotNetSdkPackage(sdk,
                releaseFile,
                this);
        }
    }

    public override Task<int> GetExpectedPackageCountAsync()
    {
        return Task.FromResult(_releases.Count);
    }

    public override Task<IPackage?> GetLatestAsync()
    {
        return Task.FromResult<IPackage?>(null);
    }

    public override Task<SemVersion?> GetLatestVersionIdAsync()
    {
        return Task.FromResult<SemVersion?>(null);
    }
}
