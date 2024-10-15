// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Extensions.CentralRepo.Runtime;

using System.IO.Abstractions;
using IceCraft.Api.Package;
using IceCraft.Extensions.CentralRepo.Network;
using IceCraft.Extensions.CentralRepo.Util;

public class MashiroStatePool : IDisposable
{
    private readonly IRemoteRepositoryManager _remoteManager;
    private Dictionary<PackageMeta, IMashiroStateLifetime>? _mashiroStates = [];
    private readonly MashiroRuntime _runtime;
    private readonly IFileSystem _fileSystem;
    private bool _disposedValue;

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
        ObjectDisposedException.ThrowIf(_disposedValue || _mashiroStates == null, this);

        if (_mashiroStates.TryGetValue(packageMeta, out var value)) return value.State;
        
        var result = await LoadLifetimeAsync(packageMeta);
        _mashiroStates.Add(packageMeta, result);
        return result.State;
    }

    private async Task<IMashiroStateLifetime> LoadLifetimeAsync(PackageMeta packageMeta)
    {
        var fileName = GetScriptFileName(packageMeta);

        var path = _fileSystem.Path.Combine(_remoteManager.LocalCachedRepoPath, "packages", fileName);

        if (!_fileSystem.File.Exists(path))
        {
            throw new InvalidOperationException("Package is non-existent on local cache");
        }
        
        return _runtime.CreateStateLifetime(await _fileSystem.File.ReadAllTextAsync(path), 
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

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _mashiroStates?.Clear();
            }

            _mashiroStates = null;
            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}