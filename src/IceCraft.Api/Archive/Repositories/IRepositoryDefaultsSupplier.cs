namespace IceCraft.Api.Archive.Repositories;

public interface IRepositoryDefaultsSupplier
{
    IEnumerable<KeyValuePair<string, Func<IServiceProvider, IRepositorySource>>> GetDefaultSources();
}
