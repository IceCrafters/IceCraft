// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Developer;

using System.Collections.Generic;
using System.Threading.Tasks;
using IceCraft.Api.Archive.Repositories;
using Semver;

public class DummyPackageSeries : IPackageSeries
{
    public string Name { get; }
    private readonly DummyPackageBuilder[] _packages;

    public DummyPackageSeries(string name, DummyPackageBuilder[] packages)
    {
        _packages = packages;
        Name = name;
    }

    public Task<int> GetExpectedPackageCountAsync()
    {
        return Task.FromResult(_packages.Length);
    }

    public Task<IPackage?> GetLatestAsync()
    {
        return Task.FromResult<IPackage?>(null);
    }

    public IEnumerable<IPackage> EnumeratePackages(CancellationToken cancellationToken = default)
    {
        foreach (var package in _packages)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return package.Build(this);
        }
    }

    public Task<SemVersion?> GetLatestVersionIdAsync()
    {
        return Task.FromResult<SemVersion?>(null);
    }
}
