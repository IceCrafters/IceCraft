// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Extensions.CentralRepo.Api;

using IceCraft.Api.Client;
using IceCraft.Extensions.CentralRepo.Runtime;
using Jint.Native;

public class MashiroConsole : IMashiroConsoleApi
{
    private readonly IOutputAdapter _output;

    public MashiroConsole(IFrontendApp app)
    {
        _output = app.Output;
    }

    public void Assert(bool assertion, string message, params JsValue[] values)
    {
        if (!assertion)
        {
            Error(message, values);
        }
    }

    public void AssertEx(bool assertion, string message, params object[] values)
    {
        if (!assertion)
        {
            ErrorEx(message, values);
        }
    }

    public void Clear()
    {
        _output.Warning("console.clear() is not supported.");
    }

    public void Debug(string message, params JsValue[] args)
    {
        _output.Verbose(MashiroFormatter.Format(message, args));
    }

    public void DebugEx(string message, params object[] args)
    {
        _output.Verbose(message, args);
    }

    public void Error(string message, params JsValue[] args)
    {
        _output.Error(MashiroFormatter.Format(message, args));
    }

    public void ErrorEx(string message, params object[] args)
    {
        _output.Error(message, args);
    }

    public void Info(string message, params JsValue[] args)
    {
        _output.Info(MashiroFormatter.Format(message, args));
    }

    public void InfoEx(string message, params object[] args)
    {
        _output.Info(message, args);
    }

    public void Log(string message, params JsValue[] args)
    {
        _output.Log(MashiroFormatter.Format(message, args));
    }

    public void LogEx(string message, params object[] args)
    {
        _output.Log(message, args);
    }

    public void Warn(string message, params JsValue[] args)
    {
        _output.Warning(MashiroFormatter.Format(message, args));
    }

    public void WarnEx(string message, params object[] args)
    {
        _output.Warning(message, args);
    }
}
