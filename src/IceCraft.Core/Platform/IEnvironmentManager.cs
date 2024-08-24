namespace IceCraft.Core.Platform;

public interface IEnvironmentManager
{
    void AddUserGlobalPath(string path);
    void AddUserGlobalPathFromHome(string relativeToHome);
}