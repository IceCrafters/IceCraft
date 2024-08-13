namespace IceCraft.Core.Installation.Storage;

using System;
using System.Text.Json;
using IceCraft.Core.Archive.Packaging;
using IceCraft.Core.Platform;
using IceCraft.Core.Serialization;
using Microsoft.Extensions.Logging;
using InstalledPackageMap = Dictionary<string, InstalledPackageInfo>;

public class PackageInstallDatabaseFactory
{
    private readonly ILogger<PackageInstallDatabaseFactory> _logger;
    private readonly IFrontendApp _frontend;

    private ValueMap? _map;

    public PackageInstallDatabaseFactory(ILogger<PackageInstallDatabaseFactory> logger,
        IFrontendApp frontend)
    {
        _logger = logger;
        _frontend = frontend;
    }

    public async Task<IPackageInstallDatabase> GetAsync()
    {
        if (_map == null)
        {
            _logger.LogInformation("Loading package database");

            var packagesPath = Path.Combine(_frontend.DataBasePath, PackageInstallManager.PackagePath);
            var retVal = await LoadDatabaseAsync(Path.Combine(packagesPath, "db.json"));
            _map = retVal;
            return retVal;
        }

        return _map;
    }

    private async Task<ValueMap> LoadDatabaseAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return await CreateDatabaseFileAsync(filePath);
        }

        // If database file exists, load existing file
        ValueMap? retVal;
        try
        {
            using var fileStream = File.OpenRead(filePath);
            retVal = await JsonSerializer.DeserializeAsync(fileStream, 
                IceCraftCoreContext.Default.PackageInstallValueMap);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Json format failure");
            return await CreateDatabaseFileAsync(filePath);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to load installation database.", ex);
        }

        if (retVal == null)
        {
            File.Delete(filePath);
            return await CreateDatabaseFileAsync(filePath);
        }

        return retVal;
    }

    private static async Task<ValueMap> CreateDatabaseFileAsync(string filePath)
    {
        var retVal = new ValueMap();
        try
        {
            using var fileStream = File.Create(filePath);
            await JsonSerializer.SerializeAsync(fileStream, retVal, IceCraftCoreContext.Default.PackageInstallValueMap);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to create installation database.", ex);
        }

        return retVal;
    }

    internal class ValueMap : InstalledPackageMap, IPackageInstallDatabase
    {
        public ValueMap()
        {
        }

        public ValueMap(IEnumerable<KeyValuePair<string, InstalledPackageInfo>> collection) : base(collection)
        {
        }

        public ValueMap(int capacity) : base(capacity)
        {
        }

        public bool ContainsMeta(PackageMeta meta)
        {
            return TryGetValue(meta.Id, out var result)
                && result.Metadata == meta;
        }

        public void Add(InstalledPackageInfo packageInfo)
        {
            Add(packageInfo.Metadata.Id, packageInfo);
        }

        public bool TryGetValue(PackageMeta meta, out InstalledPackageInfo? result)
        {
            if (!TryGetValue(meta.Id, out var info))
            {
                result = null;
                return false;
            }

            if (info.Metadata.Id != meta.Id)
            {
                result = null;
                return false;
            }

            result = info;
            return true;
        }

        public void Put(InstalledPackageInfo info)
        {
            this[info.Metadata.Id] = info;
        }
    }
}
