// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Api.Installation.Database;

using IceCraft.Api.Installation.Dependency;
using IceCraft.Api.Package;

/// <summary>
/// Provides read-only access to <see cref="IPackageInstallDatabase"/>.
/// </summary>
public interface ILocalDatabaseReadHandle
{
    InstalledPackageInfo this[string key, string version] { get; }
 
    int Count { get; }
    
    bool ContainsPackage(string identifier);
    bool ContainsPackage(PackageMeta meta);
    
    IEnumerable<string> EnumerateKeys();
    IEnumerable<PackageMeta> EnumeratePackages();
    
    InstalledPackageInfo? GetValueOrDefault(PackageMeta meta);
    InstalledPackageInfo? GetValueOrDefault(PackageReference reference);
}