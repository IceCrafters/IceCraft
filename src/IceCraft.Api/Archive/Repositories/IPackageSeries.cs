// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Api.Archive.Repositories;

using Semver;

/// <summary>
/// Represents a series of packages under the same ID but of different versions.
/// </summary>
/// <remarks>
/// <para>
/// It is recommended that implementors should be of a reference type.
/// </para>
/// </remarks>
public interface IPackageSeries
{
    string Name { get; }

    [Obsolete("Unsupported. Get latest ID and index instead.")]
    Task<IPackage?> GetLatestAsync();

    IEnumerable<IPackage> EnumeratePackages(CancellationToken cancellationToken = default);

    [Obsolete("Compare semantic versioning instead. This interface may return null.")]
    Task<SemVersion?> GetLatestVersionIdAsync();

    Task<int> GetExpectedPackageCountAsync();
}
