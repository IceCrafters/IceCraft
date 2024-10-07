// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Api.Installation.Database;

using System.Diagnostics.CodeAnalysis;
using IceCraft.Api.Installation.Dependency;
using IceCraft.Api.Package;
using Semver;

/// <summary>
/// Provides read-only access to the local package database.
/// </summary>
public interface ILocalDatabaseReadHandle
{
    InstalledPackageInfo this[string key, string version] { get; }
 
    int Count { get; }
    
    bool ContainsPackage(string identifier);
    bool ContainsPackage(string identifier, string version);
    bool ContainsPackage(string identifier, SemVersion version);
    bool ContainsPackage(PackageMeta meta);
    
    IEnumerable<string> EnumerateKeys();
    IEnumerable<PackageMeta> EnumeratePackages();
    IEnumerable<PackageMeta>? EnumeratePackagesOrDefault(string id);
    
    /// <summary>
    /// Enumerates installed package information entries with the specified ID.
    /// </summary>
    /// <param name="id">The ID.</param>
    /// <returns>An enumerable with zero or more installed package information entries under the specified ID.</returns>
    IEnumerable<InstalledPackageInfo> EnumerateEntries(string id);
    
    InstalledPackageInfo? GetValueOrDefault(PackageMeta meta);
    InstalledPackageInfo? GetValueOrDefault(PackageReference reference);
    
    bool TryGetValue(string identifier, SemVersion version, [NotNullWhen(true)] out InstalledPackageInfo? entry);
}