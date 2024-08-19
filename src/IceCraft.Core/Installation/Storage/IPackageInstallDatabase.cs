namespace IceCraft.Core.Installation.Storage;

using IceCraft.Core.Archive.Packaging;

public interface IPackageInstallDatabase
{
    PackageInstallationIndex this[string key] { get; set; }
    InstalledPackageInfo this[string key, string version] { get; set; }

    bool ContainsKey(string key);
    bool ContainsMeta(PackageMeta meta);

    void Add(InstalledPackageInfo info);
    void Add(string key, string version, InstalledPackageInfo info);

    void Put(InstalledPackageInfo info);

    void Remove(string key);
    void Remove(string key, string version);

    Task MaintainAsync();
}
