namespace IceCraft.Extensions.CentralRepo.Network;

using System.IO.Abstractions;
using IceCraft.Api.Client;
using IceCraft.Extensions.CentralRepo.Impl;
using IceCraft.Extensions.CentralRepo.Runtime;
using Jint.Runtime;

public class RemoteRepositoryIndexer
{
    private readonly RemoteRepositoryManager _remoteManager;
    private readonly IOutputAdapter _output;
    private readonly MashiroRuntime _runtime;
    private readonly IFileSystem _fileSystem;

    public RemoteRepositoryIndexer(RemoteRepositoryManager remoteManager, 
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

            var state = await _runtime.CreateStateAsync(file);

            try
            {
                state.EnsureMetadata();
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
                state.VerifyRequiredDelegates();
            }
            catch (InvalidOperationException e)
            {
                _output.Warning(e.Message);
                continue;
            }

            var meta = state.GetPackageMeta();
            if (!state.RemoteArtefact.HasValue
                || state.Origin == null
                || meta == null)
            {
                continue;
            }

            yield return new RemotePackageInfo
            {
                Artefact = state.RemoteArtefact.Value,
                Metadata = meta with
                {
                    AdditionalMetadata = new Dictionary<string, string?>
                    {
                        { "FileName", state.FileName }
                    }
                },
                Mirrors = state.GetMirrors()
            };

            state.Dispose();
        }
    }
}