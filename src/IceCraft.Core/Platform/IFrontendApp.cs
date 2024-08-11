namespace IceCraft.Core.Platform;
using System;

public interface IFrontendApp
{
    string ProductName { get; }
    string ProductVersion { get; }

    HttpClient GetClient();

    CancellationToken GetCancellationToken();
}
