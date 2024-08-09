namespace IceCraft;

using System;
using IceCraft.Core.Caching;
using IceCraft.Core.Configuration;
using Serilog;

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

    public void RemoveAll()
    {
        var directories = Directory.GetDirectories(_baseDirectory);
        Log.Verbose("Base directory: {BaseDirectory}", _baseDirectory);

        foreach (var dir in directories)
        {
            Log.Verbose("Evalutaing directory {Dir}", dir);

            if (Guid.TryParse(Path.GetFileName(dir), out _))
            {
                Directory.Delete(dir, true);
            }
        }
    }
}
