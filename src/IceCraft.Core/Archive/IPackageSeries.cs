namespace IceCraft.Core.Archive;

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

    Task<IPackage?> GetLatestAsync();
    Task<IEnumerable<IPackage>> EnumeratePackagesAsync();

    Task<string?> GetLatestVersionIdAsync();

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
