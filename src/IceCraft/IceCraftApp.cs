namespace IceCraft;

internal static class IceCraftApp
{
    internal static readonly string UserDataDirecory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "IceCraft");

    public static void Initialize()
    {
        Directory.CreateDirectory(UserDataDirecory);
    }
}
