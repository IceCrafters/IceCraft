namespace IceCraft.Api.Archive.Repositories;

public interface IRepositorySourceManager
{
    int Count { get; }

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
    Task<IEnumerable<IRepository>> GetRepositoriesAsync();
    IAsyncEnumerable<KeyValuePair<string, IRepository>> EnumerateRepositoriesAsync();
    IEnumerable<IRepositorySource> EnumerateSources();
}
