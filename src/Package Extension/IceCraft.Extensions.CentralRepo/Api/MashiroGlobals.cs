// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Extensions.CentralRepo.Api;

using IceCraft.Api.Package;
using Semver;

public static class MashiroGlobals
{
    public static SemVersion SemVer(string version)
    {
        return SemVersion.Parse(version, SemVersionStyles.Strict);
    }

    public static SemVersionRange SemRange(string range)
    {
        return SemVersionRange.Parse(range, SemVersionRangeOptions.Strict);
    }

    public static SemVersionRange SemRangeAny()
    {
        return SemVersionRange.All;
    }

    public static SemVersionRange SemRangeExact(string version)
    {
        var semversion = SemVer(version);

        return SemVersionRange.Equals(semversion);
    }

    public static SemVersionRange SemRangeAtLeast(string version)
    {
        var semversion = SemVer(version);

        return SemVersionRange.AtLeast(semversion);
    }

    public static PackageAuthorInfo Author(string name, string email)
    {
        return new PackageAuthorInfo(name, email);
    }
}