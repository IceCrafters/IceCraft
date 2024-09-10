namespace IceCraft.Api.Platform;

public interface IEnvironmentProvider
{
    /// <summary>
    /// Gets the user profile, home or other equivalent folder of the current operating system.
    /// </summary>
    /// <returns>The user profile folder.</returns>
    string GetUserProfile();
}
