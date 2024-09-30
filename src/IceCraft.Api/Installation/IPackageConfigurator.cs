// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Api.Installation;

using IceCraft.Api.Package;

public interface IPackageConfigurator
{
    Task ConfigurePackageAsync(string installDir, PackageMeta meta);
    Task UnconfigurePackageAsync(string installDir, PackageMeta meta);

    void ExportEnvironment(string installDir, PackageMeta meta);
}
