using System;
using IceCraft.Core.Platform;
using Mond.Binding;

namespace IceCraft.Extensions.CentralRepo.Api;

public class ScrOutputApi
{
    public IOutputAdapter? OutputAdapter { get; set; }

    [MondFunction("logInfo")]
    public void LogInfo(string message)
    {
        OutputAdapter?.Log(message);
    }

    [MondFunction("logWarn")]
    public void LogWarn(string message)
    {
        OutputAdapter?.Warning(message);
    }
}
