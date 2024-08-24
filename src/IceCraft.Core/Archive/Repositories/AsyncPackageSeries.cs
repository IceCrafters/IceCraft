namespace IceCraft.Core.Archive.Repositories;

using Semver;

public abstract class AsyncPackageSeries : IPackageSeries
{
    public abstract IAsyncEnumerable<IPackage> EnumeratePackagesAsync(CancellationToken cancellationToken = default);

    public abstract string Name { get; }
    
    public abstract Task<IPackage?> GetLatestAsync();

    public virtual IEnumerable<IPackage> EnumeratePackages(CancellationToken cancellationToken = default)
    {
        return EnumeratePackagesAsync(cancellationToken)
            .ToBlockingEnumerable(cancellationToken);
    }
    
    public abstract Task<SemVersion?> GetLatestVersionIdAsync();
    public abstract Task<int> GetExpectedPackageCountAsync();
}