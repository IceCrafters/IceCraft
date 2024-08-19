namespace IceCraft.Core.Installation.Storage;

using System.Collections.Generic;

public class PackageInstallationIndex : Dictionary<string, InstalledPackageInfo>
{
    public PackageInstallationIndex()
    {
    }

    public PackageInstallationIndex(IDictionary<string, InstalledPackageInfo> dictionary) : base(dictionary)
    {
    }

    public PackageInstallationIndex(IEnumerable<KeyValuePair<string, InstalledPackageInfo>> collection) : base(collection)
    {
    }

    public PackageInstallationIndex(int capacity) : base(capacity)
    {
    }
}
