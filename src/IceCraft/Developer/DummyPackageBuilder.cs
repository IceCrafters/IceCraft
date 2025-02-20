// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Developer;

using IceCraft.Api.Installation.Dependency;
using Semver;

public class DummyPackageBuilder
{
    private readonly DependencyCollection _dependencies = [];
    private SemVersion? _version;
    private bool _unitary;

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
