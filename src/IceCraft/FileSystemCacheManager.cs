namespace IceCraft;

using System;
using IceCraft.Core.Caching;
using IceCraft.Core.Configuration;

internal class FileSystemCacheManager : ICacheManager
{
    private readonly string _baseDirectory;

    public FileSystemCacheManager(IManagerConfiguration configuration)
    {
        _baseDirectory = configuration.GetCachePath();
    }

    public ICacheStorage GetStorage(Guid id)
    {
        var dir = Path.Combine(_baseDirectory, id.ToString());
        Directory.CreateDirectory(dir);

        return new FileSystemCacheStorage(id.ToString(), dir);
    }
}
