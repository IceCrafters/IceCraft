namespace IceCraft.Core.Archive;

using Semver;

/// <summary>
/// Represents a series of packages under the same ID but of different versions.
/// </summary>
/// <remarks>
/// <para>
/// It is recommended that implementors should be of a reference type.
/// </para>
/// </remarks>
public interface IPackageSeries
{
    string Name { get; }

    [Obsolete("Unsupported. Get latest ID and index instead.")]
    Task<IPackage?> GetLatestAsync();
    
    Task<IEnumerable<IPackage>> EnumeratePackagesAsync();

    Task<SemVersion?> GetLatestVersionIdAsync();

    Task<int> GetExpectedPackageCountAsync();

    async Task EnumeratePackagesAsync(Action<IPackage> consumer)
    {
        var enumerable = await EnumeratePackagesAsync();
        foreach (var x in enumerable)
        {
            consumer(x);
        }
    }
}
