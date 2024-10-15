// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Extensions.CentralRepo.Api;

using IceCraft.Api.Package;

public interface IMashiroPackagesApi
{
    PackageMeta? GetLatestInstalledPackage(string id);
    PackageMeta? GetLatestInstalledPackage(string id, bool traceVirtualProvider);
    void ImportEnvironment(PackageMeta package);
    Task RegisterVirtual(string id);
    Task RegisterVirtual(PackageMeta package);
}