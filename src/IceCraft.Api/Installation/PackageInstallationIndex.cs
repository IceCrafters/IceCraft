// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Api.Installation;

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
