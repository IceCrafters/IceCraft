// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Core.Platform.Linux;

using System.IO.Abstractions;
using System.Runtime.Versioning;
using System.Text.Json;
using IceCraft.Api.Client;
using IceCraft.Api.Platform;
using IceCraft.Core.Serialization;

/// <summary>
/// Provides environment variable management for various shells available under Linux.
/// </summary>
[SupportedOSPlatform("linux")]
public class LinuxEnvironmentManager : IEnvironmentManager
{
    private readonly string _pathFile;

    private static readonly bool DryEnvRuns = Environment.GetEnvironmentVariable("ICECRAFT_DRY_ENV") != null;

    private readonly string _pathScriptFile;
    private readonly string _envConfigFile;
    private readonly string _envScriptFile;

    private const string PathScriptFileName = ".ice_craft_paths";
    private const string PathScriptImport = $". $HOME/{PathScriptFileName}";
    
    private const string EnvConfigFileName = ".ice_craft_env_config.json";
    
    private const string EnvScriptFileName = ".ice_craft_envs";
    private const string EnvScriptImport = $". $HOME/{EnvScriptFileName}";

    private readonly Dictionary<string, string> _envRegistry;

    private readonly IFileSystem _fileSystem;

    private readonly IEnvironmentProvider _envProvider;

    private static readonly string[] ShellProfileFiles =
    [
        ".bash_profile",
        ".bash_login",
        ".profile",
        ".bashrc",
        ".zshrc"
    ];

    public LinuxEnvironmentManager(IFrontendApp frontend, IFileSystem fileSystem, IEnvironmentProvider envProvider)
    {
        var dataPath = frontend.DataBasePath;
        _pathFile = Path.Combine(dataPath, "ic_paths");

        _fileSystem = fileSystem;
        _envProvider = envProvider;

        _pathScriptFile = Path.Combine(
            _envProvider.GetUserProfile(),
            PathScriptFileName);
        _envConfigFile = Path.Combine(
            _envProvider.GetUserProfile(),
            EnvConfigFileName);
        _envScriptFile = Path.Combine(
            _envProvider.GetUserProfile(),
            EnvScriptFileName);

        _envRegistry = ReadRegistryFile();
    }

    private Dictionary<string, string> ReadRegistryFile()
    {
        if (DryEnvRuns)
        {
            return [];
        }

        if (!_fileSystem.File.Exists(_envConfigFile))
        {
            return CreateRegistryFile();
        }

        Dictionary<string, string>? result;
        using (var stream = File.OpenRead(_envConfigFile))
        {
            result = JsonSerializer.Deserialize(stream,
                IceCraftCoreContext.Default.DictionaryStringString);
        }

        return result ?? CreateRegistryFile();
    }

    private Dictionary<string, string> CreateRegistryFile()
    {
        if (DryEnvRuns)
        {
            return [];
        }

        var result = new Dictionary<string,string>();
        using var stream = _fileSystem.File.Create(_envConfigFile);
        JsonSerializer.SerializeAsync(stream, result, IceCraftCoreContext.Default.DictionaryStringString);

        return result;
    }

    private void SaveRegistryFile()
    {
        if (DryEnvRuns)
        {
            return;
        }

        using var stream = _fileSystem.File.Create(_envConfigFile);
        JsonSerializer.SerializeAsync(stream, _envRegistry, IceCraftCoreContext.Default.DictionaryStringString);
    }

    #region AddPath
    
    public void AddPath(string path, EnvironmentTarget target)
    {
        switch (target)
        {
            case EnvironmentTarget.Global:
                AddUserPath(path);
                break;
            
            case EnvironmentTarget.CurrentProcess:
                AddProcessPath(path);
                break;
            
            default:
                throw new ArgumentException("Invalid environment target.", nameof(target));
        }
    }

    private static void AddProcessPath(string path)
    {
        var processPath = Environment.GetEnvironmentVariable("PATH");
        var toPath = $"{processPath}:{path}";
        
        Environment.SetEnvironmentVariable(toPath, processPath);
    }
    
    private void AddUserPath(string path)
    {
        if (DryEnvRuns)
        {
            return;
        }

        _fileSystem.File.AppendAllLines(_pathFile, [path]);
        ApplyProfile();
    }
    
    #endregion

    #region RemovePath
    public void RemovePath(string path, EnvironmentTarget target)
    {
        switch (target)
        {
            case EnvironmentTarget.CurrentProcess:
                RemoveProcessPath(path);
                break;

            case EnvironmentTarget.Global:
                RemoveUserPath(path);
                break;

            default:
                throw new ArgumentException("Invalid environment target.", nameof(target));
        }
    }

    private static void RemoveProcessPath(string path)
    {
        var current = Environment.GetEnvironmentVariable("PATH");
        var result = current?.Replace(path, string.Empty);
        
        Environment.SetEnvironmentVariable("PATH", result);
    }
    
    private void RemoveUserPath(string path)
    {
        if (DryEnvRuns)
        {
            return;
        }

        var lines = new List<string>(File.ReadAllLines(_pathFile));
        lines.Remove(path);
        _fileSystem.File.WriteAllLines(_pathFile, lines);
        
        ApplyProfile();
    }
    #endregion

    #region SetVariable
    
    public void SetVariable(string variableName, string value, EnvironmentTarget target)
    {
        switch (target)
        {
            case EnvironmentTarget.CurrentProcess:
                Environment.SetEnvironmentVariable(variableName, value);
                break;
            
            case EnvironmentTarget.Global:
                SetUserVariable(variableName, value);
                break;
            
            default:
                throw new ArgumentException("Invalid environment target.", nameof(target));
        }
    }
    
    private void SetUserVariable(string key, string value)
    {
        if (DryEnvRuns)
        {
            return;
        }

        _envRegistry[key] = value;
        SaveRegistryFile();
        ApplyProfile();
    }
    
    #endregion

    #region RemoveVariable
    public void RemoveVariable(string variableName, EnvironmentTarget target)
    {
        switch (target)
        {
            case EnvironmentTarget.CurrentProcess:
                Environment.SetEnvironmentVariable(variableName, null);
                break;

            case EnvironmentTarget.Global:
                RemoveUserVariable(variableName);
                break;

            default:
                throw new ArgumentException("Invalid environment target.", nameof(target));
        }
    }
    
    private void RemoveUserVariable(string key)
    {
        if (DryEnvRuns)
        {
            return;
        }

        _envRegistry.Remove(key);
        SaveRegistryFile();
        ApplyProfile();
    }
    #endregion

    private void EnsurePathFile()
    {
        if (!_fileSystem.File.Exists(_pathFile))
        {
            _fileSystem.File.WriteAllLines(_pathFile, []);
        }
    }

    internal void ApplyProfile()
    {
        if (DryEnvRuns)
        {
            return;
        }

        var home = _envProvider.GetUserProfile();
        WritePathScript();
        WriteEnvScript();

        foreach (var profileFile in ShellProfileFiles)
        {
            var path = _fileSystem.Path.Combine(home, profileFile);

            if (!_fileSystem.File.Exists(path)) continue;
            if (DoesImportLineExist(path)) continue;

            _fileSystem.File.AppendAllLines(path,
            [
                "# Imports for IceCraft",
                PathScriptImport,
                EnvScriptImport
            ]);
        }
    }

    private void WriteEnvScript()
    {
        if (DryEnvRuns)
        {
            return;
        }

        using var envScript = _fileSystem.File.Create(_envScriptFile);
        var writer = new StreamWriter(envScript);
        
        writer.WriteLine("# This script is auto-generated.");
        writer.WriteLine("# Changes to contents WILL BE LOST if this file is refreshed.");

        foreach (var (key, value) in _envRegistry)
        {
            writer.WriteLine($"export {key}='{value}'");
        }
        
        writer.Flush();
    }

    private bool DoesImportLineExist(string filePath)
    {
        if (DryEnvRuns)
        {
            return true;
        }

        return _fileSystem.File.ReadLines(filePath).Any(x => x.Equals(PathScriptImport) || x.Equals(EnvScriptImport));
    }

    private void WritePathScript()
    {
        if (DryEnvRuns)
        {
            return;
        }

        using var pathScript = _fileSystem.File.Create(_pathScriptFile);
        var writer = new StreamWriter(pathScript);
        writer.WriteLine("# This script is auto-generated.");
        writer.WriteLine("# Changes to contents WILL BE LOST if this file is refreshed.");
        writer.Write("export PATH=\"$PATH");

        EnsurePathFile();
        foreach (var line in _fileSystem.File.ReadLines(_pathFile))
        {
            writer.Write(':');
            writer.Write(line);
        }
        
        writer.WriteLine('"');
        writer.WriteLine();

        writer.Flush();
    }

    public void AddUserGlobalPathFromHome(string relativeToHome)
    {
        if (DryEnvRuns)
        {
            return;
        }

        AddUserPath($"$HOME/{relativeToHome}");
    }
}