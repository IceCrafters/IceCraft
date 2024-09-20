// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Api.Platform;

public interface IEnvironmentManager
{
    void AddUserGlobalPath(string path);
    void AddUserGlobalPathFromHome(string relativeToHome);

    void RemoveUserGlobalPath(string path);

    void AddUserVariable(string key, string value);
    void RemoveUserVariable(string key);
}