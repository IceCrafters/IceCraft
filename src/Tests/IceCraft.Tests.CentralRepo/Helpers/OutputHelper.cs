// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Tests.CentralRepo.Helpers;

using System;
using IceCraft.Api.Client;
using Xunit.Abstractions;

public class OutputHelper : IOutputAdapter
{
    private readonly ITestOutputHelper _testOutput;

    public OutputHelper(ITestOutputHelper testOutput)
    {
        _testOutput = testOutput;
    }

    public void Error(string message)
    {
        _testOutput.WriteLine("IC ERR: {0}", message);
    }

    public void Error(string format, params object?[] args)
    {
        _testOutput.WriteLine($"IC ERR: {format}", args);
    }

    public void Log(string message)
    {
        _testOutput.WriteLine("IC INF: {0}", message);
    }

    public void Log(string format, params object?[] args)
    {
        _testOutput.WriteLine($"IC INF: {format}", args);
    }

    public void Tagged(string tag, string message)
    {
        _testOutput.WriteLine("IC T-{0}: {1}", tag, message);
    }

    public void Tagged(string tag, string format, params object?[] args)
    {
        _testOutput.WriteLine($"IC T-{tag}: {format}", args);
    }

    public void Verbose(string message)
    {
        _testOutput.WriteLine("IC VRB: {0}", message);
    }

    public void Verbose(string format, params object?[] args)
    {
        _testOutput.WriteLine($"IC VRB: {format}", args);
    }

    public void Warning(string message)
    {
        _testOutput.WriteLine("IC WRN: {0}", message);
    }

    public void Warning(Exception exception, string message)
    {
        _testOutput.WriteLine("IC WRN: {0}", message);
        _testOutput.WriteLine(exception.ToString());
    }

    public void Warning(string format, params object?[] args)
    {
        _testOutput.WriteLine($"IC WRN: {format}", args);
    }
}
