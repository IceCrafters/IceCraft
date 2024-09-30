// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Api.Platform;

public interface IEnvironmentManager
{
    void AddPath(string path, EnvironmentTarget target);
    void RemovePath(string path, EnvironmentTarget target);
    
    void SetVariable(string variableName, string value, EnvironmentTarget target);
    void RemoveVariable(string variableName, EnvironmentTarget target);
}