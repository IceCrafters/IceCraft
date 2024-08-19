namespace IceCraft.Core.Installation.Storage;

using System.Diagnostics.CodeAnalysis;
using IceCraft.Core.Archive.Packaging;
using Semver;

public interface IPackageInstallDatabase
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
}
