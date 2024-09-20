// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Api.Platform;

public interface IEnvironmentProvider
{
    /// <summary>
    /// Gets the user profile, home or other equivalent folder of the current operating system.
    /// </summary>
    /// <returns>The user profile folder.</returns>
    string GetUserProfile();
}
