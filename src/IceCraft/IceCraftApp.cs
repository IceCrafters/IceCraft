namespace IceCraft;

using DotNetConfig;
using IceCraft.Core;
using IceCraft.Core.Caching;
using IceCraft.Core.Configuration;

internal class IceCraftApp : IManagerDriver
{
    internal static readonly string UserDataDirecory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "IceCraft");

    public ICacheManager CachingManager { get; }

    public IManagerConfiguration Configuration { get; }

    internal IceCraftApp()
    {
        CachingManager = new FileSystemCacheManager(Path.Combine(UserDataDirecory, "caches"));
        Configuration = new DNConfigImpl(Config.Build());
    }

    public static IceCraftApp Initialize()
    {
        Directory.CreateDirectory(UserDataDirecory);
        return new IceCraftApp();
    }
}
