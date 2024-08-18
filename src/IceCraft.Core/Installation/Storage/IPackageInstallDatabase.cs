namespace IceCraft.Core.Installation.Storage;

using IceCraft.Core.Archive.Packaging;

public interface IPackageInstallDatabase
{
    InstalledPackageInfo this[string key] { get; set; }

    bool ContainsKey(string key);
    bool ContainsMeta(PackageMeta meta);

    void Add(InstalledPackageInfo info);
    void Add(string key, InstalledPackageInfo info);

    void Put(InstalledPackageInfo info);

    void Remove(string key);
}
