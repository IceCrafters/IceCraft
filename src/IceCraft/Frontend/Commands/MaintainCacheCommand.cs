namespace IceCraft.Frontend.Commands;

using System;
using System.ComponentModel;
using IceCraft.Core.Caching;
using Serilog;
using Spectre.Console.Cli;

[Description("Cleans unmapped cache files")]
public class MaintainCacheCommand : Command<MaintainCacheCommand.Settings>
{
    public class Settings : BaseSettings
    {
        [CommandOption("-N|--include-non-object")]
        [Description("Includes files that are not objects for deletion.")]
        public bool IncludeNonObject { get; set; }

        [CommandOption("-i|--inspect-only")]
        [Description("Do not delete anything, but merely tell how many files would be deleted. Overrides all delete switches.")]
        public bool InspectOnly { get; set; }
    }

    private readonly ICacheManager _cacheManager;

    public MaintainCacheCommand(ICacheManager cacheManager)
    {
        _cacheManager = cacheManager;
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        var notObjectCount = 0;
        var allCount = 0;

        var toDelete = new List<string>();

        foreach (var storage in _cacheManager.EnumerateStorages())
        {
            if (storage is not FileSystemCacheStorage fsStorage)
            {
                Log.Warning("Encounter a storage which is not a file system storage");
                continue;
            }

            Log.Verbose("Maintaining storage {Id}", fsStorage.Id);

            toDelete.EnsureCapacity(toDelete.Count + fsStorage.IndexedObjectCount);

            foreach (var file in Directory.EnumerateFiles(fsStorage.BaseDirectory))
            {
                var fileName = Path.GetFileNameWithoutExtension(file);

                // Skip our index file.
                // TODO make this a constant
                if (Path.GetFileName(file) == FileSystemCacheStorage.IndexFile)
                {
                    continue;
                }

                allCount++;

                // Check for non-object files
                if (!Guid.TryParse(fileName, out var guid))
                {
                    Log.Warning("File {FileName} in storage {Id} is not a storage object",
                        Path.GetFileName(file),
                        fsStorage.Id);
                    notObjectCount++;

                    if (settings.IncludeNonObject)
                    {
                        toDelete.Add(file);
                    }
                    continue;
                }

                // Check if object is dangling.
                if (!fsStorage.DoesMapToObject(guid))
                {
                    Log.Verbose("Storage object {Guid} is not mapped", guid);
                    toDelete.Add(file);
                    continue;
                }
            }
        }

        // Commit inspection
        if (settings.InspectOnly)
        {
            Log.Information("{Count} out of {AllCount} files would be deleted", toDelete.Count, allCount);
            return 0;
        }

        // Commit deletion
        foreach (var file in toDelete)
        {
            Log.Verbose("Deleting file {File}", file);
            File.Delete(file);
        }

        return 0;
    }
}
