namespace IceCraft.Core.Installation.Execution;

using System;
using System.IO.Abstractions;
using System.Text.Json;
using IceCraft.Core.Archive.Packaging;
using IceCraft.Core.Platform;
using IceCraft.Core.Serialization;
using Microsoft.Extensions.Logging;

public class ExecutableManager : IExecutableManager
{
    private readonly Task<Dictionary<string, ExecutableInfo>> _executables;
    private readonly IFileSystem _fileSystem;
    private readonly IPackageInstallManager _installManager;
    private readonly IExecutionScriptGenerator _scriptGenerator;
    private readonly ILogger<ExecutableManager>? _logger;
    private readonly string _dataFilePath;
    private readonly string _runPath;

    public ExecutableManager(IFrontendApp frontendApp, 
        IFileSystem fileSystem, 
        IPackageInstallManager installManager,
        IExecutionScriptGenerator scriptGenerator,
        ILogger<ExecutableManager>? logger = null)
    {
        _fileSystem = fileSystem;
        _dataFilePath = _fileSystem.Path.Combine(frontendApp.DataBasePath, "runInfo.json");
        _scriptGenerator = scriptGenerator;
        _logger = logger;

        _executables = GetDataFile(_dataFilePath);
        _runPath = _fileSystem.Path.Combine(frontendApp.DataBasePath, "run");
        _fileSystem.Directory.CreateDirectory(_runPath);

        _installManager = installManager;
    }

    public async Task RegisterAsync(PackageMeta meta, string linkName, string linkTo, EnvironmentVariableDictionary? variables = null)
    {
        var data = await _executables;
        var packageRoot = await _installManager.GetInstalledPackageDirectoryAsync(meta);

        if (_fileSystem.Path.GetInvalidFileNameChars().Any(linkName.Contains)
            || linkName.Contains("..")
            || linkName.EndsWith('\0'))
        {
            throw new ArgumentException("Link name is invalid for a file name.", nameof(linkName));
        }

        var exEntry = new ExecutableRegistrationEntry(meta.Id, linkTo, variables);
        var info = await GetInfo(linkName);

        if (info.Registrations.ContainsKey(meta.Id))
        {
            throw new ArgumentException("The specified package is already registered.", nameof(meta));
        }

        var makeDefault = info.Registrations.Count == 0;

        info.Registrations.Add(meta.Id, exEntry);
        if (makeDefault)
        {
            await SwitchAlternativeAsync(meta, linkName);
        }

        await SaveDataFile();
    }

    public async Task SwitchAlternativeAsync(PackageMeta meta, string linkName)
    {
        var packageRoot = await _installManager.GetInstalledPackageDirectoryAsync(meta);

        var info = await GetInfo(linkName);

        if (!info.Registrations.TryGetValue(meta.Id, out var registerInfo))
        {
            throw new ArgumentException("The specified executable was not registered.", nameof(meta));
        }

        var tempFileName = _fileSystem.Path.Combine(_runPath, _fileSystem.Path.GetRandomFileName());
        var linkFileName = _fileSystem.Path.Combine(_runPath, linkName);
        var targetName = _fileSystem.Path.GetFullPath(registerInfo.LinkTarget, packageRoot);

        using (var stream = _fileSystem.File.Create(tempFileName))
        {
            await _scriptGenerator.WriteExecutionScriptAsync(registerInfo, targetName, stream);
        }

        if (!OperatingSystem.IsWindows())
        {
            var fileMode = File.GetUnixFileMode(tempFileName);
            if (!fileMode.HasFlag(UnixFileMode.UserExecute))
            {
                fileMode |= UnixFileMode.UserExecute;
            }
            
            File.SetUnixFileMode(tempFileName, fileMode);
        }

        // TODO Implement a better overwrite system
        _fileSystem.File.Move(tempFileName, linkFileName, true);
        info.Current = registerInfo;
        await SaveDataFile();
    }

    private async Task<ExecutableInfo> GetInfo(string linkName)
    {
        var data = await _executables;
        if (!data.TryGetValue(linkName, out var result))
        {
            var retVal = new ExecutableInfo()
            {
                Registrations = new Dictionary<string, ExecutableRegistrationEntry>()
            };
            data.Add(linkName, retVal);
            return retVal;
        }

        return result;
    }

    public async Task<bool> UnlinkExecutableAsync(string linkName)
    {
        var data = await _executables;
        if (!data.TryGetValue(linkName, out var execInfo))
        {
            return false;
        }

        var target = _fileSystem.Path.Combine(_runPath, linkName);
        if (_fileSystem.File.Exists(target))
        {
            _fileSystem.File.Delete(target);
        }

        execInfo.Current = null;
        await SaveDataFile();
        return true;
    }

    public async Task UnregisterAsync(PackageMeta meta, string linkName)
    {
        var info = await GetInfo(linkName);
        info.Registrations.Remove(meta.Id);
        if (info.Current?.PackageRef == meta.Id)
        {
            await UnlinkExecutableAsync(linkName);
            info.Current = null;
        }

        await SaveDataFile();
    }

    #region Data File Management
    private async Task<Dictionary<string, ExecutableInfo>> GetDataFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return await CreateNewDataFile(filePath);
        }

        try
        {
            Dictionary<string, ExecutableInfo>? retVal;

            await using (var stream = _fileSystem.File.OpenRead(filePath))
            {
                retVal = await JsonSerializer.DeserializeAsync(stream,
                    IceCraftCoreContext.Default.ExecutableDataFile_v2);
            }

            if (retVal == null)
            {
                return await CreateNewDataFile(filePath);
            }

            return retVal;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Unable to create configuration", ex);
        }
    }

    private async Task<Dictionary<string, ExecutableInfo>> CreateNewDataFile(string filePath)
    {
        using var stream = _fileSystem.File.Create(filePath);
        var retVal = new Dictionary<string, ExecutableInfo>();
        await JsonSerializer.SerializeAsync(stream, retVal, IceCraftCoreContext.Default.ExecutableDataFile_v2);
        return retVal;
    }

    private async Task SaveDataFile()
    {
        var data = await _executables;

        using var stream = _fileSystem.File.Create(_dataFilePath);
        await JsonSerializer.SerializeAsync(stream, data, IceCraftCoreContext.Default.ExecutableDataFile_v2);
    }
    #endregion
}
