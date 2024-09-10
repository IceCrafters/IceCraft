namespace IceCraft;

using System.IO;
using System.Text.Json;
using IceCraft.Api.Caching;
using IceCraft.Core.Caching;
using IceCraft.Frontend;
using Serilog;

internal class FileSystemCacheStorage : ICacheStorage
{
    private readonly Dictionary<string, Guid> _cacheIndex;
    private bool _dry;
    private readonly string _indexFilePath;

    public const string IndexFile = "index.json";

    internal FileSystemCacheStorage(string id, string baseDirectory)
    {
        ArgumentException.ThrowIfNullOrEmpty(baseDirectory);
        ArgumentException.ThrowIfNullOrEmpty(id);

        if (!Directory.Exists(baseDirectory))
        {
            throw new DirectoryNotFoundException("Base directory is either non-existent or is not a directory.");
        }

        BaseDirectory = baseDirectory;
        Id = id;

        _indexFilePath = Path.Combine(BaseDirectory, IndexFile);
        _cacheIndex = InitializeCacheIndex();
    }

    public string BaseDirectory { get; }

    private Dictionary<string, Guid> InitializeCacheIndex()
    {
        if (File.Exists(_indexFilePath))
        {
            try
            {
                using var stream = File.OpenRead(_indexFilePath);
                var index = JsonSerializer.Deserialize<Dictionary<string, Guid>>(stream);
                if (index != null)
                {
                    return index;
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to read cache index for storage: '{}'", Id);
            }
        }

        var dict = new Dictionary<string, Guid>();

        try
        {
            using var stream = File.Create(_indexFilePath);
            JsonSerializer.Serialize(stream, dict);
        }
        catch // (Exception ex)
        {
            Output.Shared.Warning("WARNING: Failed to create cache index for storage: '{}'", Id);
            Output.Shared.Warning("WARNING: Cache will not be stored on disk.");
            _dry = true;
        }

        return dict;
    }

    public Stream CreateObject(string objectName)
    {
        if (_dry)
        {
            Log.Warning("WARNING: Cache object '{}' will not be saved", objectName);
            var mem = new MemoryStream();
            return mem;
        }

        return InternalOverwriteObject(objectName);
    }

    public void DeleteObject(string objectName)
    {
        if (!_cacheIndex.TryGetValue(objectName, out var id))
        {
            return;
        }

        var file = InternalGetFileName(id);
        if (File.Exists(file))
        {
            File.Delete(file);
        }

        _cacheIndex.Remove(objectName);
        InternalSaveIndex();
    }

    public bool DoesObjectExist(string objectName)
    {
        return _cacheIndex.TryGetValue(objectName, out var id)
               && File.Exists(InternalGetFileName(id));
    }

    public Stream OpenReadObject(string objectName)
    {
        if (!_cacheIndex.TryGetValue(objectName, out var id))
        {
            throw new ArgumentException("The specified object was not found.", nameof(objectName));
        }

        var file = InternalGetFileName(id);
        if (!File.Exists(file))
        {
            throw new ArgumentException("The specified object does not refer to an on-disk file.", nameof(objectName));
        }

        return File.OpenRead(file);
    }

    #region Helpers

    public int IndexedObjectCount => _cacheIndex.Count;

    public string Id { get; }

    public bool DoesMapToObject(Guid guid)
    {
        return _cacheIndex.ContainsValue(guid);
    }

    private FileStream InternalOverwriteObject(string objectName)
    {
        var guid = Guid.NewGuid();
        _cacheIndex[objectName] = guid;
        InternalSaveIndex();
        return File.Create(InternalGetFileName(guid));
    }

    private string InternalGetFileName(Guid guid)
    {
        return Path.Combine(BaseDirectory, $"{guid}.cache");
    }

    private void InternalSaveIndex()
    {
        using (var stream = File.Create(_indexFilePath))
        {
            JsonSerializer.Serialize(stream, _cacheIndex);
        }

        _dry = false;
    }

    public void Clear()
    {
        foreach (var obj in _cacheIndex)
        {
            var file = InternalGetFileName(obj.Value);
            if (File.Exists(file))
            {
                File.Delete(file);
            }
        }

        _cacheIndex.Clear();
        InternalSaveIndex();
    }

    #endregion
}
