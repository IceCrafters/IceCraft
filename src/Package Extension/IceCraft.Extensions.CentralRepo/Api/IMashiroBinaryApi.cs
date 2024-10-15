// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Extensions.CentralRepo.Api;

using IceCraft.Api.Platform;

public interface IMashiroBinaryApi
{
    Task Register(string fileName, string path);
    Task Register(string fileName, string path, EnvironmentVariableDictionary envVars);
    Task Unregister(string fileName);
}