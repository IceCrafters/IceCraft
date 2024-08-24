namespace IceCraft.Core.Platform.Linux;

using System.Runtime.Versioning;
using System.Text.Json;
using IceCraft.Core.Serialization;

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
    private static readonly string EnvConfigFile = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        EnvConfigFileName);
    private static readonly string EnvScriptFile = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        EnvScriptFileName);

    private const string PathScriptFileName = ".ice_craft_paths";
    private const string PathScriptImport = $"source $HOME/{PathScriptFileName}";
    
    private const string EnvConfigFileName = ".ice_craft_env_config.json";
    
    private const string EnvScriptFileName = ".ice_craft_envs";
    private const string EnvScriptImport = $"source $HOME/{EnvScriptFileName}";

    private readonly Dictionary<string, string> _envRegistry;

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

        _envRegistry = ReadRegistryFile();
    }

    private static Dictionary<string, string> ReadRegistryFile()
    {
        if (!File.Exists(EnvConfigFile))
        {
            return CreateRegistryFile();
        }

        Dictionary<string, string>? result;
        using (var stream = File.OpenRead(EnvConfigFile))
        {
            result = JsonSerializer.Deserialize(stream,
                IceCraftCoreContext.Default.DictionaryStringString);
        }

        return result ?? CreateRegistryFile();
    }

    private static Dictionary<string, string> CreateRegistryFile()
    {
        var result = new Dictionary<string,string>();
        using var stream = File.Create(EnvConfigFile);
        JsonSerializer.SerializeAsync(stream, result, IceCraftCoreContext.Default.DictionaryStringString);

        return result;
    }

    private void SaveRegistryFile()
    {
        using var stream = File.Create(EnvConfigFile);
        JsonSerializer.SerializeAsync(stream, _envRegistry, IceCraftCoreContext.Default.DictionaryStringString);
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
        WriteEnvScript();

        foreach (var profileFile in ShellProfileFiles)
        {
            var path = Path.Combine(home, profileFile);

            if (!File.Exists(path)) continue;
            if (DoesImportLineExist(path)) continue;

            File.AppendAllLines(path,
            [
                "# Imports for IceCraft",
                PathScriptImport,
                EnvScriptImport
            ]);
        }
    }

    private void WriteEnvScript()
    {
        using var envScript = File.Create(EnvScriptFile);
        var writer = new StreamWriter(envScript);
        
        writer.WriteLine("# This script is auto-generated.");
        writer.WriteLine("# Changes to contents WILL BE LOST if this file is refreshed.");

        foreach (var (key, value) in _envRegistry)
        {
            writer.WriteLine($"export {key}='{value}'");
        }
    }

    private static bool DoesImportLineExist(string filePath)
    {
        return File.ReadLines(filePath).Any(x => x.Equals(PathScriptImport) || x.Equals(EnvScriptImport));
    }

    private void WritePathScript()
    {
        using var pathScript = File.Create(PathScriptFile);
        var writer = new StreamWriter(pathScript);
        writer.WriteLine("# This script is auto-generated.");
        writer.WriteLine("# Changes to contents WILL BE LOST if this file is refreshed.");
        writer.Write("export PATH='");

        foreach (var line in File.ReadLines(_pathFile))
        {
            writer.Write(line);
            writer.Write(':');
        }
        
        writer.WriteLine('\'');
        writer.WriteLine();
    }

    public void AddUserGlobalPathFromHome(string relativeToHome)
    {
        AddUserGlobalPath($"$HOME/{relativeToHome}");
    }

    public void RemoveUserGlobalPath(string path)
    {
        var lines = new List<string>(File.ReadAllLines(_pathFile));
        lines.Remove(path);
        File.WriteAllLines(_pathFile, lines);
        
        ApplyProfile();
    }

    public void AddUserVariable(string key, string value)
    {
        _envRegistry[key] = value;
        SaveRegistryFile();
        ApplyProfile();
    }

    public void RemoveUserVariable(string key)
    {
        _envRegistry.Remove(key);
        SaveRegistryFile();
        ApplyProfile();
    }
}