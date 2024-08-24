namespace IceCraft.Core.Platform.Linux;

using System.Runtime.Versioning;

/// <summary>
/// Provides environment variable management for various shells available under Linux.
/// </summary>
[SupportedOSPlatform("linux")]
public class LinuxEnvironmentManager : IEnvironmentManager
{
    private readonly string _pathFile;

    private static readonly string PathScriptFile = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        PathScriptFileName);

    private const string PathScriptFileName = ".ice_craft_envs";
    private const string PathScriptImport = $"source $HOME/{PathScriptFileName}";

    private static readonly string[] ShellProfileFiles =
    [
        ".bash_profile",
        ".bash_login",
        ".profile"
    ];

    public LinuxEnvironmentManager(IFrontendApp frontend)
    {
        var dataPath = frontend.DataBasePath;
        _pathFile = Path.Combine(dataPath, "ic_paths");
    }

    public void AddUserGlobalPath(string path)
    {
        File.AppendAllLines(_pathFile, [path]);
        ApplyProfile();
    }

    private void ApplyProfile()
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        WritePathScript();

        foreach (var profileFile in ShellProfileFiles)
        {
            var path = Path.Combine(home, profileFile);

            if (!File.Exists(path)) continue;
            if (DoesImportLineExist(path)) continue;

            File.AppendAllLines(path,
            [
                "# Imports for IceCraft",
                PathScriptImport
            ]);
        }
    }

    private static bool DoesImportLineExist(string filePath)
    {
        return File.ReadLines(filePath).Any(x => x.Equals(PathScriptImport));
    }

    private void WritePathScript()
    {
        using var pathScript = File.Create(PathScriptFile);
        var writer = new StreamWriter(pathScript);
        writer.WriteLine("# This script is auto-generated.");
        writer.WriteLine("# Changes to contents WILL BE LOST if this file is refreshed.");

        foreach (var line in File.ReadLines(_pathFile))
        {
            writer.Write(line);
            writer.Write(':');
        }

        writer.WriteLine();
    }

    public void AddUserGlobalPathFromHome(string relativeToHome)
    {
        AddUserGlobalPath($"$HOME/{relativeToHome}");
    }
}