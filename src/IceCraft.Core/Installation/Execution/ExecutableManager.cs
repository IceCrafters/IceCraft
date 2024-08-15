namespace IceCraft.Core.Installation.Execution;

using System;
using System.IO.Abstractions;
using System.Text.Json;
using IceCraft.Core.Archive.Packaging;
using IceCraft.Core.Platform;
using IceCraft.Core.Serialization;

public class ExecutableManager : IExecutableManager
{
    private readonly Task<Dictionary<string, ExecutableEntry>> _executables;
    private readonly IFileSystem _fileSystem;
    private readonly IPackageInstallManager _installManager;
    private readonly IExecutionScriptGenerator _scriptGenerator;
    private readonly string _dataFilePath;
    private readonly string _runPath;

    public ExecutableManager(IFrontendApp frontendApp, 
        IFileSystem fileSystem, 
        IPackageInstallManager installManager,
        IExecutionScriptGenerator scriptGenerator)
    {
        _fileSystem = fileSystem;
        _dataFilePath = _fileSystem.Path.Combine(frontendApp.DataBasePath, "runInfo.json");
        _scriptGenerator = scriptGenerator;

        _executables = GetDataFile(_dataFilePath);
        _runPath = _fileSystem.Path.Combine(frontendApp.DataBasePath, "run");
        _fileSystem.Directory.CreateDirectory(_runPath);

        _installManager = installManager;
    }

    public async Task LinkExecutableAsync(PackageMeta meta, string linkName, string linkTarget, EnvironmentVariableDictionary? variables = null)
    {
        var data = await _executables;
        if (_fileSystem.Path.GetInvalidFileNameChars().Any(linkName.Contains)
            || linkName.Contains("..")
            || linkName.EndsWith('\0'))
        {
            throw new ArgumentException("Link name is invalid for a file name.", nameof(linkName));
        }

        var packageRoot = await _installManager.GetInstalledPackageDirectoryAsync(meta);

        var exEntry = new ExecutableEntry()
        {
            LinkName = linkName,
            LinkTarget = linkTarget,
            PackageRef = meta.Id,
            Variables = variables
        };

        // Set the link data if necessary.
        data.Remove(linkName);
        data.Add(linkName, exEntry);

        var tempFileName = _fileSystem.Path.Combine(_runPath, _fileSystem.Path.GetRandomFileName());
        var linkFileName = _fileSystem.Path.Combine(_runPath, linkName);
        var targetName = _fileSystem.Path.GetFullPath(linkTarget, packageRoot);

        using (var stream = _fileSystem.File.Create(tempFileName))
        {
            await _scriptGenerator.WriteExecutionScriptAsync(exEntry, targetName, stream);
        }

        // TODO Implement a better overwrite system
        _fileSystem.File.Move(tempFileName, linkFileName, true);
        await SaveDataFile();
    }

    public async Task<bool> UnlinkExecutableAsync(string linkName)
    {
        var data = await _executables;
        if (!data.ContainsKey(linkName))
        {
            return false;
        }

        var target = _fileSystem.Path.Combine(_runPath, linkName);
        if (_fileSystem.File.Exists(target))
        {
            _fileSystem.File.Delete(target);
        }

        data.Remove(linkName);
        await SaveDataFile();
        return true;
    }

    #region Data File Management
    private async Task<Dictionary<string, ExecutableEntry>> GetDataFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return await CreateNewDataFile(filePath);
        }

        try
        {
            Dictionary<string, ExecutableEntry>? retVal;

            await using (var stream = _fileSystem.File.OpenRead(filePath))
            {
                retVal = await JsonSerializer.DeserializeAsync(stream,
                    IceCraftCoreContext.Default.ExecutableDataFile);
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

    private async Task<Dictionary<string, ExecutableEntry>> CreateNewDataFile(string filePath)
    {
        using var stream = _fileSystem.File.Create(filePath);
        var retVal = new Dictionary<string, ExecutableEntry>();
        await JsonSerializer.SerializeAsync(stream, retVal, IceCraftCoreContext.Default.ExecutableDataFile);
        return retVal;
    }

    private async Task SaveDataFile()
    {
        var data = await _executables;

        using var stream = _fileSystem.File.Create(_dataFilePath);
        await JsonSerializer.SerializeAsync(stream, data, IceCraftCoreContext.Default.ExecutableDataFile);
    }
    #endregion
}
