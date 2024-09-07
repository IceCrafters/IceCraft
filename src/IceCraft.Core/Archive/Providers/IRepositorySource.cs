namespace IceCraft.Core.Archive.Providers;

/// <summary>
/// Defines a supplier for a single repository.
/// </summary>
/// <remarks>
/// It is expected that sources only to initialize cache, request version information etc. when creating and regenerating
/// repositories. Doing initialization in constructors or in field initialization can cause caches to unnecessarily generate
/// and process even if the source is disabled, and can prevent <see cref="RefreshAsync"/> from performing its purpose: 
/// reload and regenerate package version cache.
/// </remarks>
public interface IRepositorySource
{
    /// <summary>
    /// Creates a new instance of the repository provided by this provider, reusing the previous cache if available.
    /// </summary>
    /// <remarks>
    /// Repositories should cache their data and only regenerate on <see cref="RefreshAsync"/>, and
    /// when first initialization.
    /// </remarks>
    /// <returns>The created repository, or <see langword="null"/> if no repository can be provided.</returns>
    Task<IRepository?> CreateRepositoryAsync();

    [Obsolete("Use Refresh instead.")]
    async Task<IRepository?> RegenerateRepository()
    {
        await RefreshAsync();
        return await CreateRepositoryAsync();
    }

    /// <summary>
    /// Deletes all cached data, and regenerate everything at the next <see cref="CreateRepositoryAsync"/> call.
    /// </summary>
    Task RefreshAsync();
}
