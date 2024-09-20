// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Api.Platform;

using IceCraft.Api.Package;

public interface IExecutableManager
{
    Task RegisterAsync(PackageMeta meta, string linkName, string linkTo, EnvironmentVariableDictionary? variables = null);

    Task UnregisterAsync(PackageMeta meta, string linkName);

    Task SwitchAlternativeAsync(PackageMeta meta, string linkName);

    /// <summary>
    /// Deletes the link in the executables directory of the specified name.
    /// </summary>
    /// <param name="linkName"></param>
    /// <returns><see langword="true"/> if the link exists and have been removed; otherwise, <see langword="false"/>.</returns>
    Task<bool> UnlinkExecutableAsync(string linkName);
}
