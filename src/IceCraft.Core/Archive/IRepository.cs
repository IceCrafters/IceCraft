namespace IceCraft.Core.Archive;

public interface IRepository
{
    /// <summary>
    /// Gets the series with the specified name.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <returns>The returned series; <see langword="null"/> if not found.</returns>
    IPackageSeries? GetSeriesOrDefault(string name);

    /// <summary>
    /// Enumerate all series available for this repository.
    /// </summary>
    /// <returns>An enumeratable for the series.</returns>
    IEnumerable<IPackageSeries> EnumerateSeries();

    int GetExpectedSeriesCount();
}
