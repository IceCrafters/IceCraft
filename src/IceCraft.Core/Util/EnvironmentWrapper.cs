namespace IceCraft.Core.Util;

using IceCraft.Api.Platform;
using Environment = System.Environment;

public class EnvironmentWrapper : IEnvironmentProvider
{
    public string GetUserProfile()
    {
        return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    }
}
