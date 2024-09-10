namespace IceCraft.Api.Client;

using IceCraft.Api.Network;

public interface IFrontendApp
{
    string ProductName { get; }
    string ProductVersion { get; }

    string DataBasePath { get; }

    IOutputAdapter Output { get; }

    HttpClient GetClient();

    CancellationToken GetCancellationToken();

    Task DoProgressedTaskAsync(string description, Func<IProgressedTask, Task> action);

    [Obsolete("Use DoProgressedTaskAsync instead.")]
    Task DoDownloadTaskAsync(Func<INetworkDownloadTask, Task> action);

    Task DoStatusTaskAsync(string initialStatus, Func<IStatusReporter, Task> action);
}
