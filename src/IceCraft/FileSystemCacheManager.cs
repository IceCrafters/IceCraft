namespace IceCraft;

using System;
using System.Collections.Generic;
using IceCraft.Api.Caching;
using IceCraft.Api.Client;
using IceCraft.Core.Caching;
using Serilog;

internal class FileSystemCacheManager : ICacheManager
{
    private readonly string _baseDirectory;

    public FileSystemCacheManager(IManagerConfiguration configuration)
    {
        _baseDirectory = configuration.GetCachePath();
    }

    public IEnumerable<ICacheStorage> EnumerateStorages()
    {
        var directories = Directory.GetDirectories(_baseDirectory);
        Log.Verbose("Base directory: {BaseDirectory}", _baseDirectory);
        var list = new List<ICacheStorage>(directories.Length);

        foreach (var dir in directories)
        {
            Log.Verbose("Evalutaing directory {Dir}", dir);

            if (Guid.TryParse(Path.GetFileName(dir), out var id))
            {
                list.Add(GetStorage(id));
            }
        }

        return list.AsReadOnly();
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
