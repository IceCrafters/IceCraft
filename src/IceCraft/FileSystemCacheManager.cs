namespace IceCraft;

using System;
using IceCraft.Core.Caching;

internal class FileSystemCacheManager : ICachingManager
{
    private readonly string _baseDirectory;

    public FileSystemCacheManager(string baseDirectory)
    {
        _baseDirectory = baseDirectory;
    }

    public ICacheStorage GetStorage(Guid id)
    {
        var dir = Path.Combine(_baseDirectory, id.ToString());
        Directory.CreateDirectory(dir);

        return new FileSystemCacheStorage(id.ToString(), dir);
    }
}
