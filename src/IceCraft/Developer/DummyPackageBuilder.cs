namespace IceCraft.Developer;

using IceCraft.Core.Archive.Dependency;
using Semver;

public class DummyPackageBuilder
{
    private readonly DependencyCollection _dependencies = [];
    private SemVersion? _version;
    private bool _unitary;

    public DummyPackageBuilder()
    {
    }

    public DummyPackageBuilder WithVersion(SemVersion version)
    {
        _version = version;
        return this;
    }

    public DummyPackageBuilder WithDependency(string name, SemVersionRange versionRange)
    {
        _dependencies.Add(new DependencyReference(name, versionRange));
        return this;
    }

    public DummyPackage Build(DummyPackageSeries series)
    {
        if (_version == null)
        {
            throw new InvalidOperationException("Incomplete package.");
        }

        return new DummyPackage(series.Name, series, _version, _dependencies, _unitary);
    }

    public DummyPackageBuilder Unitary()
    {
        _unitary = true;
        return this;
    }
}
