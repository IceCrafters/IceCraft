namespace IceCraft.Core.Archive.Repositories;

using IceCraft.Core.Archive.Providers;

public interface IRepositoryDefaultsSupplier
{
    IEnumerable<KeyValuePair<string, Func<IServiceProvider, IRepositorySource>>> GetDefaultSources();
}
