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

    async Task EnumeratePackagesAsync(Action<IPackage> consumer)
    {
        var enumerables = await EnumeratePackagesAsync();
        foreach (var x in enumerables)
        {
            consumer(x);
        }
    }
}
