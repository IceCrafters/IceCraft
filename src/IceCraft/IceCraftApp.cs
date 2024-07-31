namespace IceCraft;

internal static class IceCraftApp
{
    internal static readonly string UserDataDirecory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "IceCraft");
    internal static readonly string CachesDirectory = Path.Combine(UserDataDirecory, "caches");

    public static void Initialize()
    {
        Directory.CreateDirectory(UserDataDirecory);
    }
}
