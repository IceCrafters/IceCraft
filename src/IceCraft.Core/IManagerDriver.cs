namespace IceCraft.Core;

using IceCraft.Core.Caching;

/// <summary>
/// Defines an interface for the user interface driver of the package manager.
/// </summary>
public interface IManagerDriver
{
    /// <summary>
    /// Gets a shared instance of cache manager implementation.
    /// </summary>
    ICacheManager CachingManager { get; }
}
