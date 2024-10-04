// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Extensions.CentralRepo.Runtime;

using System.IO.Abstractions;
using IceCraft.Api.Package;
using IceCraft.Extensions.CentralRepo.Network;
using IceCraft.Extensions.CentralRepo.Util;

public class MashiroStatePool
{
    private readonly IRemoteRepositoryManager _remoteManager;
    private readonly Dictionary<PackageMeta, MashiroState> _mashiroStates = new();
    private readonly MashiroRuntime _runtime;
    private readonly IFileSystem _fileSystem;

    public MashiroStatePool(IRemoteRepositoryManager remoteManager, 
        MashiroRuntime runtime,
        IFileSystem fileSystem)
    {
        _remoteManager = remoteManager;
        _runtime = runtime;
        _fileSystem = fileSystem;
    }
    
    public async Task<MashiroState> GetAsync(PackageMeta packageMeta)
    {
        if (_mashiroStates.TryGetValue(packageMeta, out var value)) return value;
        
        var result = await LoadLocalStateAsync(packageMeta);
        _mashiroStates.Add(packageMeta, result);
        return result;
    }

    private async Task<MashiroState> LoadLocalStateAsync(PackageMeta packageMeta)
    {
        var fileName = GetScriptFileName(packageMeta);

        var path = _fileSystem.Path.Combine(_remoteManager.LocalCachedRepoPath, "packages", fileName);
        Console.WriteLine(path);

        if (!_fileSystem.File.Exists(path))
        {
            throw new InvalidOperationException("Package is non-existent on local cache");
        }
        
        return _runtime.CreateState(await _fileSystem.File.ReadAllTextAsync(path), 
            Path.GetFileNameWithoutExtension(path));
    }

    internal static string GetScriptFileName(PackageMeta packageMeta)
    {
        var fileName = $"{packageMeta.Id}-{packageMeta.Version}.js";
        if (packageMeta.CustomData != null
            && packageMeta.CustomData.TryGetValueDeserialize(RemoteRepositoryIndexer.RemoteRepoData, 
                CsrJsonContext.Default.RemotePackageData, 
                out var packageData)
            && packageData is { FileName: not null })
        {
            fileName = packageData.FileName.EndsWith(".js") ? 
                packageData.FileName :
                $"{packageData.FileName}.js";
        }

        return fileName;
    }
}