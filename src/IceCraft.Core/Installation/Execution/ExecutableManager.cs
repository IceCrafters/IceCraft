namespace IceCraft.Core.Installation.Execution;

using System;
using System.Text.Json;
using IceCraft.Core.Archive.Packaging;
using IceCraft.Core.Platform;
using IceCraft.Core.Serialization;

public class ExecutableManager : IExecutableManager
{
    private readonly Task<Dictionary<string, ExecutableEntry>> _executables;
    private readonly string _dataFilePath;
    private readonly string _runPath;

    public ExecutableManager(IFrontendApp frontendApp)
    {
        _dataFilePath = Path.Combine(frontendApp.DataBasePath, "runInfo.json");

        _executables = GetDataFile(_dataFilePath);
        _runPath = Path.Combine(frontendApp.DataBasePath, "run");
        Directory.CreateDirectory(_runPath);
    }

    public async Task LinkExecutableAsync(PackageMeta meta, string linkName, string from)
    {
        var data = await _executables;
        if (Path.GetInvalidFileNameChars().Any(linkName.Contains)
            || linkName.Contains("..")
            || linkName.EndsWith('\0'))
        {
            throw new ArgumentException("Link name is invalid for a file name.", nameof(linkName));
        }

        data.Remove(linkName);
        data.Add(linkName, new ExecutableEntry()
        {
            LinkName = linkName,
            LinkTarget = from,
            PackageRef = meta.Id
        });

        var tempFileName = Path.Combine(_runPath, Path.GetRandomFileName());
        var target = Path.Combine(_runPath, linkName);

        File.CreateSymbolicLink(tempFileName, from);

        if (OperatingSystem.IsLinux())
        {
            File.Move(tempFileName, target, true);
        }
        else if (!File.Exists(target))
        {
            File.Move(tempFileName, target);
        }
        else
        {
            throw new NotImplementedException("Support for overwriting link files are not implemented yet.");
        }
        await SaveDataFile();
    }

    public async Task<bool> UnlinkExecutableAsync(string linkName)
    {
        var data = await _executables;
        if (!data.ContainsKey(linkName))
        {
            return false;
        }

        var target = Path.Combine(_runPath, linkName);
        if (File.Exists(target))
        {
            File.Delete(target);
        }

        data.Remove(linkName);
        await SaveDataFile();
        return true;
    }

    #region Data File Management
    private static async Task<Dictionary<string, ExecutableEntry>> GetDataFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return await CreateNewDataFile(filePath);
        }

        try
        {
            Dictionary<string, ExecutableEntry>? retVal;

            await using (var stream = File.OpenRead(filePath))
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

    private static async Task<Dictionary<string, ExecutableEntry>> CreateNewDataFile(string filePath)
    {
        using var stream = File.Create(filePath);
        var retVal = new Dictionary<string, ExecutableEntry>();
        await JsonSerializer.SerializeAsync(stream, retVal, IceCraftCoreContext.Default.ExecutableDataFile);
        return retVal;
    }

    private async Task SaveDataFile()
    {
        var data = await _executables;

        using var stream = File.Create(_dataFilePath);
        await JsonSerializer.SerializeAsync(stream, data, IceCraftCoreContext.Default.ExecutableDataFile);
    }
    #endregion
}
