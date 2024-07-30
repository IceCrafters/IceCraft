namespace IceCraft;

using System.IO;
using System.Text.Json;
using IceCraft.Core.Caching;

internal class FileSystemCacheStorage : ICacheStorage
{
    private readonly string _baseDirectory;
    private readonly string _id;
    private readonly Dictionary<string, Guid> _cacheIndex;
    private bool _dry;
    private readonly string _indexFilePath;

    internal FileSystemCacheStorage(string id, string baseDirectory)
    {
        ArgumentException.ThrowIfNullOrEmpty(baseDirectory);
        ArgumentException.ThrowIfNullOrEmpty(id);

        if (!Directory.Exists(baseDirectory))
        {
            throw new DirectoryNotFoundException("Base directory is either non-existent or is not a directory.");
        }

        _baseDirectory = baseDirectory;
        _id = id;

        _indexFilePath = Path.Combine(_baseDirectory, "index.json");
        _cacheIndex = InitializeCacheIndex();
    }

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
                Console.WriteLine("WARNING: Failed to read cache index for storage: '{0}'", _id);
                Console.WriteLine(ex);
            }
        }

        var dict = new Dictionary<string, Guid>();

        try
        {
            using var stream = File.Create(_indexFilePath);
            JsonSerializer.Serialize(stream, dict);
        }
        catch (Exception ex)
        {
            Console.WriteLine("WARNING: Failed to create cache index for storage: '{0}'", _id);
            Console.WriteLine("WARNING: Cache will not be stored on disk.");
            _dry = true;
            Console.WriteLine(ex);
        }

        return dict;
    }

    public Stream CreateObject(string objectName)
    {
        if (_dry)
        {
            Console.WriteLine("WARNING: Cache object '{0}' will not be saved", objectName);
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
        return;
    }

    public bool DoesObjectExist(string objectName)
    {
        if (!_cacheIndex.TryGetValue(objectName, out var id))
        {
            return false;
        }

        if (!File.Exists(InternalGetFileName(id)))
        {
            return false;
        }

        return true;
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

    private FileStream InternalOverwriteObject(string objectName)
    {
        var guid = Guid.NewGuid();
        _cacheIndex[objectName] = guid;
        InternalSaveIndex();
        return File.Create(InternalGetFileName(guid));
    }

    private string InternalGetFileName(Guid guid)
    {
        return Path.Combine(_baseDirectory, $"{guid}.cache");
    }

    private void InternalSaveIndex()
    {
        using (var stream = File.Create(_indexFilePath))
        {
            JsonSerializer.Serialize(stream, _cacheIndex);
        }

        _dry = false;
    }

    #endregion
}
