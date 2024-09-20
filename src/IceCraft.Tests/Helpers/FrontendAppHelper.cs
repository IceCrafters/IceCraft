// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Tests.Helpers;

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using IceCraft.Api.Client;
using IceCraft.Api.Network;
using Xunit.Abstractions;

public class FrontendAppHelper : IFrontendApp
{
    private readonly CancellationTokenSource _cancelSource;

    public FrontendAppHelper(ITestOutputHelper outputHelper)
    {
        Output = new OutputHelper(outputHelper);
        _cancelSource = new CancellationTokenSource();
    }

    public string ProductName => "Test XUnit Instance";

    public string ProductVersion => "0.0.0";

    public string DataBasePath => Path.Combine(Path.GetTempPath(), "__icecraft_test_temp");

    public IOutputAdapter Output { get; }

    [Obsolete]
    public Task DoDownloadTaskAsync(Func<INetworkDownloadTask, Task> action)
    {
        throw new NotImplementedException();
    }

    public Task DoProgressedTaskAsync(string description, Func<IProgressedTask, Task> action)
    {
        throw new NotImplementedException();
    }

    public Task DoStatusTaskAsync(string initialStatus, Func<IStatusReporter, Task> action)
    {
        throw new NotImplementedException();
    }

    public CancellationToken GetCancellationToken()
    {
        return _cancelSource.Token;
    }

    public HttpClient GetClient()
    {
        throw new NotImplementedException();
    }

    public void Cancel()
    {
        _cancelSource.Cancel();
    }
}
