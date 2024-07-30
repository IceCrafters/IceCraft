namespace IceCraft;

using IceCraft.Core;
using IceCraft.Core.Caching;

internal class IceCraftApp : IManagerDriver
{
    internal static readonly string UserDataDirecory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "IceCraft");

    public ICachingManager CachingManager { get; }

    internal IceCraftApp()
    {
        CachingManager = new FileSystemCacheManager(Path.Combine(UserDataDirecory, "caches"));
    }

    public static void Initialize()
    {
        Directory.CreateDirectory(UserDataDirecory);
    }
}
