namespace IceCraft.Core.Network;

using System.Diagnostics;
using System.Net;
using IceCraft.Core.Archive.Artefacts;
using IceCraft.Core.Platform;
using Microsoft.Extensions.Logging;

public class MirrorSearcher : IMirrorSearcher
{
    private readonly ILogger<MirrorSearcher> _logger;
    private readonly IFrontendApp _frontend;

    public MirrorSearcher(ILogger<MirrorSearcher> logger, IFrontendApp frontend)
    {
        _logger = logger;
        _frontend = frontend;
    }

    public async Task<ArtefactMirrorInfo?> GetBestMirrorAsync(IEnumerable<ArtefactMirrorInfo>? mirrors)
    {
        var client = _frontend.GetClient();

        if (mirrors == null)
        {
            return null;
        }

        ArtefactMirrorInfo? bestMirror = null;
        long bestPing = 0;

        var stop = new Stopwatch();

        foreach (var mirror in mirrors)
        {
            _logger.LogDebug("PRB: {Name} ({Host})", mirror.Name, mirror.DownloadUri.Host);

            HttpResponseMessage response;

            try
            {
                stop.Restart();
                response = await client.GetAsync(mirror.DownloadUri);
                stop.Stop();
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                _logger.LogWarning("Timed out on mirror {Name}", mirror.Name);
                continue;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning("Can't connect to mirror {Name}: {Message}", mirror.Name, ex.Message);
                continue;
            }

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    _logger.LogTrace("Mirror {Name}: resource not found", mirror.Name);
                }
                else
                {
                    _logger.LogTrace("Mirror {Name} returned status code {StatusCode}", mirror.Name, response.StatusCode);
                }
            }

            var ping = stop.ElapsedMilliseconds;
            if (bestMirror == null || ping < bestPing)
            {
                bestMirror = mirror;
                bestPing = ping;
                continue;
            }

            _logger.LogTrace("Mirror {Name}: {Ping}ms", mirror.Name, ping);

            response.Dispose();
        }

        if (bestMirror == null)
        {
            return null;
        }

        _logger.LogTrace("Best mirror: {Name} ({BestPing}ms)", bestMirror.Name, bestPing);
        return bestMirror;
    }
}
