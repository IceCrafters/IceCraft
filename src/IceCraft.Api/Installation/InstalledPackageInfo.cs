// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Api.Installation;

using IceCraft.Api.Installation.Dependency;
using IceCraft.Api.Package;

public sealed record InstalledPackageInfo
{
    public required PackageMeta Metadata { get; init; }
    public required InstallationState State { get; set; }
    public PackageReference? ProvidedBy { get; init; }
    
    /// <summary>
    /// Gets a value indicating whether this package is installed explicitly rather than
    /// being installed as a dependency.
    /// </summary>
    public bool IsExplicitlyInstalled { get; init; }
}
