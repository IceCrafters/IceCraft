// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Extensions.CentralRepo.Impl;

using IceCraft.Api.Archive.Repositories;
using IceCraft.Extensions.CentralRepo.Network;
using Semver;

public class RemotePackageSeries : IPackageSeries
{
    private readonly IReadOnlyList<RemotePackageInfo> _packages;

    public RemotePackageSeries(string id, IReadOnlyList<RemotePackageInfo> packages)
    {
        Name = id;
        _packages = packages;
    }

    public string Name { get; }

    public Task<IPackage?> GetLatestAsync()
    {
        return Task.FromResult<IPackage?>(null);
    }

    public IEnumerable<IPackage> EnumeratePackages(CancellationToken cancellationToken = default)
    {
        foreach (var package in _packages)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return new RemotePackage(package, this);
        }
    }

    public Task<SemVersion?> GetLatestVersionIdAsync()
    {
        return Task.FromResult<SemVersion?>(null);
    }

    public Task<int> GetExpectedPackageCountAsync()
    {
        return Task.FromResult(_packages.Count);
    }
}