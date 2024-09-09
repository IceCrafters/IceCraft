namespace IceCraft.Frontend.Commands;

using System.CommandLine;
using IceCraft.Core.Caching;

public class CliCacheCommandFactory
{
    private readonly ICacheManager _cacheManager;

    public CliCacheCommandFactory(ICacheManager cacheManager)
    {
        _cacheManager = cacheManager;
    }
    
    public Command CreateCli()
    {
        var command = new Command("cache", "Manage and maintains cache data")
        {
            CreateClearCommand(),
            CreateMaintainCommand()
        };

        return command;
    }

    #region Maintain command
    private Command CreateMaintainCommand()
    {
        var optNonObject = new Option<bool>("--include-non-object", "Deletes cache files that aren't registered");
        var optDryRun = new Option<bool>("--dry-run", "Only show those would be deleted but don't actually delete them");

        var command = new Command("maintain", "Removes unused cache files")
        {
            optNonObject,
            optDryRun
        };
        command.SetHandler(ExecuteMaintainCommand, optNonObject, optDryRun);
        return command;
    }

    private void ExecuteMaintainCommand(bool includeNonObject, bool inspectOnly)
    {
        var allCount = 0;

        var toDelete = new List<string>();

        foreach (var storage in _cacheManager.EnumerateStorages())
        {
            if (storage is not FileSystemCacheStorage fsStorage)
            {
                Output.Shared.Warning("Encounter a storage which is not a file system storage");
                continue;
            }

            Output.Shared.Verbose("Maintaining storage {0}", fsStorage.Id);

            toDelete.EnsureCapacity(toDelete.Count + fsStorage.IndexedObjectCount);

            foreach (var file in Directory.EnumerateFiles(fsStorage.BaseDirectory))
            {
                var fileName = Path.GetFileNameWithoutExtension(file);

                // Skip our index file.
                if (Path.GetFileName(file) == FileSystemCacheStorage.IndexFile)
                {
                    continue;
                }

                allCount++;

                // Check for non-object files
                if (!Guid.TryParse(fileName, out var guid))
                {
                    Output.Shared.Warning("File {0} in storage {1} is not a storage object",
                        Path.GetFileName(file),
                        fsStorage.Id);

                    if (includeNonObject)
                    {
                        toDelete.Add(file);
                    }
                    continue;
                }

                // Check if object is dangling.
                // ReSharper disable once InvertIf
                if (!fsStorage.DoesMapToObject(guid))
                {
                    Output.Shared.Verbose("Storage object {0} is not mapped", guid);
                    toDelete.Add(file);
                }
            }
        }

        // Commit inspection
        if (inspectOnly)
        {
            Output.Shared.Log("{0} out of {1} files would be deleted", toDelete.Count, allCount);
        }

        // Commit deletion
        foreach (var file in toDelete)
        {
            Output.Shared.Verbose("Deleting file {0}", file);
            File.Delete(file);
        }
    }
    #endregion

    private Command CreateClearCommand()
    {
        var command = new Command("clear", "Clears all cache");
        command.SetHandler(_ => _cacheManager.RemoveAll());
        return command;
    }
}