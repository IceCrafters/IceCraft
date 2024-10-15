// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Extensions.CentralRepo.Api;
using Jint.Native;

public interface IMashiroConsoleApi
{
    void Assert(bool assertion, string message, params JsValue[] values);
    void AssertEx(bool assertion, string message, params object[] values);

    void Clear();

    void Debug(string message, params JsValue[] args);
    void DebugEx(string message, params object[] args);
    void Error(string message, params JsValue[] args);
    void ErrorEx(string message, params object[] args);
    void Info(string message, params JsValue[] args);
    void InfoEx(string message, params object[] args);
    void Log(string message, params JsValue[] args);
    void LogEx(string message, params object[] args);
    void Warn(string message, params JsValue[] args);
    void WarnEx(string message, params object[] args);
}
