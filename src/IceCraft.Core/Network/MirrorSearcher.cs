namespace IceCraft.Core.Network;

using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
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
        if (mirrors == null)
        {
            return null;
        }

        ArtefactMirrorInfo? bestMirror = null;
        long bestPing = 0;

        var stop = new Stopwatch();
        var pingSender = new Ping();

        foreach (var mirror in mirrors)
        {
            _logger.LogDebug("PRB: {Name} ({Host})", mirror.Name, mirror.DownloadUri.Host);

            PingReply reply;

            try
            {
                reply = await pingSender.SendPingAsync(mirror.DownloadUri.Host);
            }
            catch (PingException ex)
            {
                _logger.LogWarning("Can't connect to mirror {Name}: {Message}", mirror.Name, ex.Message);
                continue;
            }

            if (reply.Status != IPStatus.Success)
            {
                _logger.LogWarning("Ping failure {Name}: {Status}", mirror.Name, reply.Status);
                continue;
            }

            var ping = reply.RoundtripTime;
            if (bestMirror == null || ping < bestPing)
            {
                bestMirror = mirror;
                bestPing = ping;
                continue;
            }

            _logger.LogTrace("Mirror {Name}: {Ping}ms", mirror.Name, ping);
        }

        if (bestMirror == null)
        {
            return null;
        }

        _logger.LogTrace("Best mirror: {Name} ({BestPing}ms)", bestMirror.Name, bestPing);
        return bestMirror;
    }
}
