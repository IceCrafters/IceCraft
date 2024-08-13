namespace IceCraft.Core.Platform;
using System;
using IceCraft.Core.Network;

public interface IFrontendApp
{
    string ProductName { get; }
    string ProductVersion { get; }

    string DataBasePath { get; }

    HttpClient GetClient();

    CancellationToken GetCancellationToken();

    Task DoProgressedTaskAsync(string description, Func<IProgressedTask, Task> action);
    Task DoDownloadTaskAsync(Func<INetworkDownloadTask, Task> action);
}
