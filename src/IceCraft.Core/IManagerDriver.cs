namespace IceCraft.Core;

using IceCraft.Core.Caching;
using IceCraft.Core.Configuration;

/// <summary>
/// Defines an interface for the user interface driver of the package manager.
/// </summary>
public interface IManagerDriver
{
    /// <summary>
    /// Gets a shared instance of cache manager implementation.
    /// </summary>
    ICacheManager CachingManager { get; }

    /// <summary>
    /// Gets a shared instance of configuration.
    /// </summary>
    IManagerConfiguration Configuration { get; }
}
