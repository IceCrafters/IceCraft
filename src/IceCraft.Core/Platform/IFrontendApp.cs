using System;

namespace IceCraft.Core.Platform;

public interface IFrontendApp
{
    string ProductName { get; }
    string ProductVersion { get; }

    HttpClient GetClient();
}
