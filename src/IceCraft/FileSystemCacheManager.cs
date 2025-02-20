﻿// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft;

using System;
using System.Collections.Generic;
using IceCraft.Api.Caching;
using IceCraft.Api.Client;
using IceCraft.Frontend;
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
        Output.Shared.Verbose("Cache base directory: {0}", _baseDirectory);
        var list = new List<ICacheStorage>(directories.Length);

        foreach (var dir in directories)
        {
            Output.Shared.Verbose("Evaluating cache directory {0}", dir);

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
        Output.Shared.Verbose("base directory: {0}", _baseDirectory);

        foreach (var dir in directories)
        {
            Output.Shared.Verbose("evalutaing directory {0}", dir);

            if (Guid.TryParse(Path.GetFileName(dir), out _))
            {
                Directory.Delete(dir, true);
            }
        }
    }
}
