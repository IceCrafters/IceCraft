// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Core.Platform.Windows;

using System.Runtime.Versioning;
using IceCraft.Api.Platform;

/// <summary>
/// Provides environment variable management for operating systems with <c>Win32</c> API (i.e. Windows).
/// </summary>
[SupportedOSPlatform("windows")]
public class WindowsEnvironmentManager : IEnvironmentManager
{
    private static EnvironmentVariableTarget GetVariableTarget(EnvironmentTarget target)
    {
        return target switch
        {
            EnvironmentTarget.CurrentProcess => EnvironmentVariableTarget.Process,
            EnvironmentTarget.Global => EnvironmentVariableTarget.User,
            _ => throw new ArgumentException("Invalid environment target.", nameof(target))
        };
    }
    
    #region Path Manager
    
    private static void AddPathToEnv(string path, EnvironmentVariableTarget target)
    {
        var currentPath = Environment.GetEnvironmentVariable("PATH", target);
        if (currentPath != null && currentPath.EndsWith(';'))
        {
            currentPath = currentPath[..^1];
        }

        var newPath = $"{currentPath};{path}";
        Environment.SetEnvironmentVariable("PATH", newPath, target);
    }
    
    private static void RemovePathFromEnv(string path, EnvironmentVariableTarget target)
    {
        var currentPath = Environment.GetEnvironmentVariable("PATH", target);
        if (currentPath != null && currentPath.EndsWith(';'))
        {
            currentPath = currentPath[..^1];
        }

        var newPath = currentPath?.Replace($"{path};", "")
            .Replace(path, "");
        Environment.SetEnvironmentVariable("PATH", newPath, target);
    }
    
    #endregion

    public void AddPath(string path, EnvironmentTarget target)
    {
        var varTarget = GetVariableTarget(target);
        AddPathToEnv(path, varTarget);
    }

    public void RemovePath(string path, EnvironmentTarget target)
    {
        var varTarget = GetVariableTarget(target);
        RemovePathFromEnv(path, varTarget);
    }

    public void SetVariable(string variableName, string value, EnvironmentTarget target)
    {
        var env = GetVariableTarget(target);
        
        Environment.SetEnvironmentVariable(variableName, value, env);
    }

    public void RemoveVariable(string variableName, EnvironmentTarget target)
    {
        var env = GetVariableTarget(target);
        
        Environment.SetEnvironmentVariable(variableName, null, env);
    }

    public void AddUserGlobalPathFromHome(string relativeToHome)
    {
        AddPathToEnv(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), relativeToHome),
            EnvironmentVariableTarget.User);
    }
}