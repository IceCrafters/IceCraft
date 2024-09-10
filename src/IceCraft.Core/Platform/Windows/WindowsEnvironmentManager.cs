namespace IceCraft.Core.Platform.Windows;

using System.Runtime.Versioning;
using IceCraft.Api.Platform;

/// <summary>
/// Provides environment variable management for operating systems with <c>Win32</c> API (i.e. Windows).
/// </summary>
[SupportedOSPlatform("windows")]
public class WindowsEnvironmentManager : IEnvironmentManager
{
    public void AddUserGlobalPath(string path)
    {
        var currentPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User);
        if (currentPath != null && currentPath.EndsWith(';'))
        {
            currentPath = currentPath[..^1];
        }

        var newPath = $"{currentPath};{path}";
        Environment.SetEnvironmentVariable("PATH", newPath, EnvironmentVariableTarget.User);
    }

    public void AddUserGlobalPathFromHome(string relativeToHome)
    {
        AddUserGlobalPath(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), relativeToHome));
    }

    public void RemoveUserGlobalPath(string path)
    {
        var currentPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User);
        if (currentPath != null && currentPath.EndsWith(';'))
        {
            currentPath = currentPath[..^1];
        }

        var newPath = currentPath?.Replace($"{path};", "")
            .Replace(path, "");
        Environment.SetEnvironmentVariable("PATH", newPath, EnvironmentVariableTarget.User);
    }

    public void AddUserVariable(string key, string value)
    {
        Environment.SetEnvironmentVariable(key, value, EnvironmentVariableTarget.User);
    }

    public void RemoveUserVariable(string key)
    {
        Environment.SetEnvironmentVariable(key, null, EnvironmentVariableTarget.User);
    }
}