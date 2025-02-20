// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Extensions.CentralRepo.Network;

using System.IO.Abstractions;
using IceCraft.Api.Client;
using IceCraft.Api.Package.Data;
using IceCraft.Extensions.CentralRepo.Impl;
using IceCraft.Extensions.CentralRepo.Runtime;
using IceCraft.Extensions.CentralRepo.Util;
using Jint.Runtime;

public class RemoteRepositoryIndexer
{
    private readonly IRemoteRepositoryManager _remoteManager;
    private readonly IOutputAdapter _output;
    private readonly MashiroRuntime _runtime;
    private readonly IFileSystem _fileSystem;
    
    internal const string RemoteRepoData = "remote_repo_data";

    public RemoteRepositoryIndexer(IRemoteRepositoryManager remoteManager, 
        IFrontendApp frontend, 
        MashiroRuntime runtime, 
        IFileSystem fileSystem)
    {
        _remoteManager = remoteManager;
        _output = frontend.Output;
        _runtime = runtime;
        _fileSystem = fileSystem;
    }

    internal async Task<(int, IEnumerable<RemotePackageSeries>)> IndexSeries()
    {
        var dict = new Dictionary<string, List<RemotePackageInfo>>();

        await foreach (var package in IndexPackagesAsync())
        {
            if (dict.TryGetValue(package.Metadata.Id, out var list))
            {
                list.Add(package);
                continue;
            }

            var newList = new List<RemotePackageInfo> { package };
            dict.Add(package.Metadata.Id, newList);
        }

        return (dict.Count, dict.Select(x => new RemotePackageSeries(x.Key, x.Value)));
    }

    private async IAsyncEnumerable<RemotePackageInfo> IndexPackagesAsync()
    {
        var baseDir = _fileSystem.Path.Combine(_remoteManager.LocalCachedRepoPath, "packages");
        if (!_fileSystem.Directory.Exists(baseDir))
        {
            yield break;
        }

        foreach (var file in _fileSystem.Directory.EnumerateFiles(baseDir))
        {
            if (!file.EndsWith(".js"))
            {
                continue;
            }

            var lifetime = await _runtime.CreateStateLifetimeAsync(file);

            try
            {
                lifetime.State.EnsureMetadata();
            }
            catch (JavaScriptException e)
            {
                _output.Warning("Execution failure for {0}", e.Location.SourceFile);
                _output.Warning(e.Error.ToString());
                _output.Warning(e.JavaScriptStackTrace ?? "<none>");
                continue;
            }

            try
            {
                lifetime.State.VerifyRequiredDelegates();
            }
            catch (InvalidOperationException e)
            {
                _output.Warning(e.Message);
                continue;
            }

            var meta = lifetime.State.GetPackageMeta();
            if (lifetime.State.ArtefactDefinition == null
                || lifetime.State.Origin == null
                || meta == null)
            {
                continue;
            }

            var customData = new PackageCustomDataDictionary();
            customData.AddSerialize(RemoteRepoData, new RemotePackageData(Path.GetFileNameWithoutExtension(file)),
                CsrJsonContext.Default.RemotePackageData);
            
            yield return new RemotePackageInfo
            {
                Artefact = lifetime.State.ArtefactDefinition,
                Metadata = meta with
                {
                    CustomData = customData
                },
                Mirrors = lifetime.State.GetMirrors()
            };

            lifetime.Dispose();
        }
    }
}