// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Api.Platform;

public enum EnvironmentTarget
{
    /// <summary>
    /// The environment is local to the current process.
    /// </summary>
    CurrentProcess,
    /// <summary>
    /// The environment is global to the current user.
    /// </summary>
    Global
}