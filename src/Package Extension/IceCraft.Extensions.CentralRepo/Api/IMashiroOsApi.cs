// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Extensions.CentralRepo.Api;

public interface IMashiroOsApi
{
    /// <summary>
    /// Executes an executable with the specified arguments.
    /// </summary>
    /// <param name="program">The program to execute.</param>
    /// <param name="args">The arguments to specify for the program.</param>
    /// <returns>The exit code, or <c>-255</c> if the process have failed to start.</returns>
    int Execute(string program, params string[] args);

    int System(string command);
    void SetProcessEnv(string key, string value);
    void RemoveProcessEnv(string key);
    void AddProcessPath(string path);
    void RemoveProcessPath(string path);
}