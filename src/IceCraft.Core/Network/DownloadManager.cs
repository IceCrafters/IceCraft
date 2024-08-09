namespace IceCraft.Core.Network;

using System;
using IceCraft.Core.Platform;

public class DownloadManager : IDownloadManager
{
    private readonly HttpClient _httpClient;

    public DownloadManager(IFrontendApp frontendApp)
    {
        _httpClient = frontendApp.GetClient();
    }

    public async Task Download(Uri from, string toFile, INetworkDownloadTask? task = null)
    {
        var response = await _httpClient.GetAsync(from);

        var dlTask = new DownloadTask(response, toFile, task);
        await dlTask.Perform();
    }
}
