namespace IceCraft.Core.Archive.Providers;

public interface IRepositoryProvider
{
    /// <summary>
    /// Creates a new instance of the repository provided by this provider, reusing the previous cache if available.
    /// </summary>
    /// <remarks>
    /// Repositories should cache their data and only regenerate on <see cref="RegenerateRepository"/>, and
    /// when first initialization.
    /// </remarks>
    /// <returns>The created repository, or <see langword="null"/> if no repository can be provided.</returns>
    Task<IRepository?> CreateRepository();

    Task<IRepository?> RegenerateRepository();
}
