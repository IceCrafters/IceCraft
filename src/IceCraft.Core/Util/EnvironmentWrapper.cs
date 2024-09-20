// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Core.Util;

using IceCraft.Api.Platform;
using Environment = System.Environment;

public class EnvironmentWrapper : IEnvironmentProvider
{
    public string GetUserProfile()
    {
        return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    }
}
