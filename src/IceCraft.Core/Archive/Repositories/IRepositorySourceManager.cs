namespace IceCraft.Core;

using System.Threading.Tasks;
using IceCraft.Core.Archive;
using IceCraft.Core.Archive.Providers;

public interface IRepositorySourceManager
{
    /// <summary>
    /// Registers the specified instance as a source.
    /// </summary>
    /// <param name="id">The ID of the source.</param>
    /// <param name="source">The source to register.</param>
    void RegisterSource(string id, IRepositorySource source);

    /// <summary>
    /// Registers the specified source, acquired as a keyed service,
    /// from the service provider associated with the instance.
    /// </summary>
    /// <param name="key">The service key. Will be used as its ID.</param>
    void RegisterSourceAsService(string key);

    bool ContainsSource(string id);
    Task<IEnumerable<IRepository>> GetRepositories();
    IEnumerable<IRepositorySource> EnumerateSources();
}
