// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Extensions.CentralRepo.Network;

using IceCraft.Api.Client;
using IceCraft.Api.Exceptions;
using IceCraft.Extensions.CentralRepo.Util;
using SharpCompress.Common;
using SharpCompress.Readers;

public class RemoteRepositoryManager : IRemoteRepositoryManager
{
    private readonly IFrontendApp _frontendApp;
    private readonly IOutputAdapter _output;
    private readonly RepoConfigFactory _configFactory;

    public RemoteRepositoryManager(IFrontendApp frontendApp, RepoConfigFactory configFactory)
    {
        _frontendApp = frontendApp;
        _configFactory = configFactory;
        _output = frontendApp.Output;

        var csrDataPath = Path.Combine(frontendApp.DataBasePath, "csr");
        LocalCachedRepoPath = Path.Combine(csrDataPath, "repository");
    }

    public string LocalCachedRepoPath { get; }

    public void CleanCached()
    {
        if (Directory.Exists(LocalCachedRepoPath))
        {
            Directory.Delete(LocalCachedRepoPath, true);
        }
    }

    public FileStream GetAssetFileStream(string assetName)
    {
        var invalids = Path.GetInvalidFileNameChars();
        if (assetName.Any(x => invalids.Contains(x)))
        {
            throw new ArgumentException("Invalid asset filename.", nameof(assetName));
        }

        var assetPath = Path.Combine(LocalCachedRepoPath, "assets", assetName);
        if (!File.Exists(assetPath))
        {
            throw new FileNotFoundException("Failed to locate asset.", assetName);
        }

        return File.OpenRead(assetPath);
    }

    public async Task InitializeCacheAsync()
    {
        if (Directory.Exists(LocalCachedRepoPath))
        {
            return;
        }

        Directory.CreateDirectory(LocalCachedRepoPath);

        string downloadedPath;
        await using (var archiveStream = await GetArchiveStreamAsync())
        {
            downloadedPath = await DownloadArchiveTempAsync(archiveStream);
        }

        var effective = GetEffectiveRepository();

        await using var stream = File.OpenRead(downloadedPath);
        using var reader = ReaderFactory.Open(stream);

        var options = new ExtractionOptions
        {
            ExtractFullPath = true,
            Overwrite = true
        };

        if (effective.Subfolder != null
        #if DEBUG
        // Disallow subfolder structure for debug targz file
        && Environment.GetEnvironmentVariable("ICECRAFT_DBG_CSR_ARCHIVE_PATH") == null
        #endif
        )
        {
            var tmp = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tmp);
            reader.WriteAllToDirectory(tmp, options);

            FsUtil.CopyDirectory(Path.Combine(tmp, effective.Subfolder), LocalCachedRepoPath, true);
        }
        else
        {
            reader.WriteAllToDirectory(LocalCachedRepoPath, options);
        }
    }

    private async Task<string> DownloadArchiveTempAsync(Stream stream)
    {
        _output.Log("Downloading repository information...");

        var tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        await using var objective = File.Create(tempPath);
        await stream.CopyToAsync(objective);

        return tempPath;
    }

    private async Task<Stream> GetArchiveStreamAsync()
    {
#if DEBUG
        var envVar = Environment.GetEnvironmentVariable("ICECRAFT_DBG_CSR_ARCHIVE_PATH");
        if (envVar != null && File.Exists(envVar))
        {
            return File.OpenRead(envVar);
        }
#endif
        var repo = GetEffectiveRepository();

        var client = _frontendApp.GetClient();
        var response = await client.GetAsync(repo.Uri);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStreamAsync();
    }

    private RemoteRepositoryInfo GetEffectiveRepository()
    {
        var value = _configFactory.GetData();
        return value.Repository;
    }
}