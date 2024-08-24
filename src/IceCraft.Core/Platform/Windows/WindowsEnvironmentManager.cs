namespace IceCraft.Core.Platform.Windows;

using System.Runtime.Versioning;

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
}