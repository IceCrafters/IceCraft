// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Core.Network;

using System.Diagnostics;
using System.Net.NetworkInformation;
using IceCraft.Api.Archive.Artefacts;
using IceCraft.Api.Client;
using IceCraft.Api.Network;

public class MirrorSearcher : IMirrorSearcher
{
    private readonly IFrontendApp _frontend;

    public MirrorSearcher(IFrontendApp frontend)
    {
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

        var pingSender = new Ping();

        foreach (var mirror in mirrors)
        {
            PingReply reply;

            try
            {
                reply = await pingSender.SendPingAsync(mirror.DownloadUri.Host);
            }
            catch (PingException ex)
            {
                _frontend.Output.Warning("Can't connect to mirror {0}: {1}", mirror.Name, ex.Message);
                continue;
            }

            if (reply.Status != IPStatus.Success)
            {
                _frontend.Output.Warning("Ping failure {0}: {1}", mirror.Name, reply.Status);
                continue;
            }

            var ping = reply.RoundtripTime;
            if (bestMirror == null || ping < bestPing)
            {
                bestMirror = mirror;
                bestPing = ping;
                continue;
            }

            _frontend.Output.Verbose("Mirror {0}: {1}ms", mirror.Name, ping);
        }

        if (bestMirror == null)
        {
            return null;
        }

        _frontend.Output.Verbose("Best mirror: {0} ({1}ms)", bestMirror.Name, bestPing);
        return bestMirror;
    }
}
