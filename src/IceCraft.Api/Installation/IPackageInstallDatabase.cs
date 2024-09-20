// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Api.Installation;

using System.Diagnostics.CodeAnalysis;
using IceCraft.Api.Installation.Dependency;
using IceCraft.Api.Package;
using Semver;

public interface IPackageInstallDatabase : IEnumerable<KeyValuePair<string, PackageInstallationIndex>>
{
    PackageInstallationIndex this[string key] { get; set; }
    InstalledPackageInfo this[string key, string version] { get; set; }

    bool ContainsKey(string key);
    bool ContainsMeta(PackageMeta meta);

    void Add(InstalledPackageInfo info);
    void Add(string key, SemVersion version, InstalledPackageInfo info);

    void Put(InstalledPackageInfo info);

    void Remove(string key);
    void Remove(string key, SemVersion version);

    Task MaintainAsync();
    bool TryGetValue(string packageName, [NotNullWhen(true)] out PackageInstallationIndex? index);

    IEnumerable<string> EnumerateKeys();

    IEnumerable<PackageMeta> EnumeratePackages();
    
    PackageInstallationIndex? GetValueOrDefault(string packageId);

    InstalledPackageInfo? GetValueOrDefault(PackageMeta meta);

    InstalledPackageInfo? GetValueOrDefault(PackageReference reference);
    
    /// <summary>
    /// Gets the total number of package indices currently contained in the database.
    /// </summary>
    int Count { get; }
}
