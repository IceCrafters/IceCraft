namespace IceCraft.Repositories.Adoptium;

using IceCraft.Core.Archive;

public class AdoptiumPackageSeries : IPackageSeries
{
    private readonly int _majorVersion;
    private readonly string _type;
    private readonly AdoptiumRepository _repository;

    internal AdoptiumPackageSeries(int majorVersion, string type, AdoptiumRepository repository)
    {
        _majorVersion = majorVersion;
        _type = type;
        Name = $"adoptium{_majorVersion}-{type}";
        _repository = repository;
    }

    public string Name { get; }
}
