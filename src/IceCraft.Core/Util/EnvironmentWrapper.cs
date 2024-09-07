namespace IceCraft.Core.Util;

public class EnvironmentWrapper : IEnvironmentProvider
{
    public string GetUserProfile()
    {
        return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    }
}
