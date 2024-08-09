namespace IceCraft.Core.Caching;

/// <summary>
/// Defines a manager which provides management for caches.
/// </summary>
public interface ICacheManager
{
    /// <summary>
    /// Gets a storage for non-volatile storage of various metadata caches. If the cache does not exist yet,
    /// it will be created.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Callers should not download and store software binary or source packages in caches. These are expected to be
    /// handled by the package manager through artefacts system, which handles download, storing and lifetime of artefact
    /// objects.
    /// </para>
    /// </remarks>
    /// <param name="id">The ID of the storage to acquire. Should be unique.</param>
    /// <returns>The acquired storage.</returns>
    ICacheStorage GetStorage(Guid id);

    void RemoveAll();
}
