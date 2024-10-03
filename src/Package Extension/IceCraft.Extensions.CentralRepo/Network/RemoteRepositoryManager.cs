// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Extensions.CentralRepo.Network;

using IceCraft.Api.Client;
using IceCraft.Api.Exceptions;
using SharpCompress.Common;
using SharpCompress.Readers;

public class RemoteRepositoryManager
{
    private readonly IFrontendApp _frontendApp;
    private readonly IOutputAdapter _output;
    private readonly ICustomConfig _customConfig;

    private static readonly RemoteRepositoryInfo OfficialRepository = new RemoteRepositoryInfo(
        new Uri("https://gitlab.com/icecrafters/repository/-/archive/main/repository-main.tar.gz"),
        "repository-main"
    );

    public RemoteRepositoryManager(IFrontendApp frontendApp, ICustomConfig customConfig)
    {
        _frontendApp = frontendApp;
        _customConfig = customConfig;
        _output = frontendApp.Output;

        var csrDataPath = Path.Combine(frontendApp.DataBasePath, "csr");
        LocalCachedRepoPath = Path.Combine(csrDataPath, "repository");
    }

    internal string LocalCachedRepoPath { get; }

    internal void CleanPrevious()
    {
        if (Directory.Exists(LocalCachedRepoPath))
        {
            Directory.Delete(LocalCachedRepoPath, true);
        }
    }

    internal async Task InitializeCacheAsync()
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

        var extractEntry = false;

        // If we have a subfolder specified then try to extract subfolder
        if (effective.Subfolder != null 
        #if DEBUG
        // Disallow subfolder structure for debug targz file
        && Environment.GetEnvironmentVariable("ICECRAFT_DBG_CSR_ARCHIVE_PATH") == null
        #endif
        )
        {
            while (reader.Entry.Key != effective.Subfolder)
            {
                if (!reader.MoveToNextEntry())
                {
                    throw new KnownInvalidOperationException($"Subfolder '{effective.Subfolder}' specified in config doesn't exist!");
                }
            }

            extractEntry = true;
        }

        var options = new ExtractionOptions
        {
            ExtractFullPath = true,
            Overwrite = true
        };

        if (extractEntry)
        {
            reader.WriteEntryToDirectory(LocalCachedRepoPath, options);
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

    private RemoteRepositoryInfo GetEffectiveRepository()
    {
        return GetUserSpecifiedRepository()
                   ?? OfficialRepository;
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

    private RemoteRepositoryInfo? GetUserSpecifiedRepository()
    {
        var scope = _customConfig.GetScope("csr");

        var sourceUriStr = scope.Get("repo_uri");
        var sourceSubfolder = scope.Get("repo_subfolder");

        if (string.IsNullOrWhiteSpace(sourceUriStr)
        || !Uri.TryCreate(sourceUriStr, UriKind.Absolute, out var sourceUri))
        {
            return null;
        }

        return new RemoteRepositoryInfo(sourceUri, sourceSubfolder);
    }
}