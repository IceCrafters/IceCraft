// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Api.Installation;

using IceCraft.Api.Package;

public interface IPackageInstallHook
{
    void OnBeforePackageExpand(PackageMeta meta, string expandTo, string installPath);
    void OnBeforePackageConfigure(PackageMeta meta, string installPath);
    void OnPackageInstalled(PackageMeta meta, string installPath);
}
