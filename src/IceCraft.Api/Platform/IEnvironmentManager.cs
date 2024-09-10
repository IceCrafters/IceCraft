namespace IceCraft.Api.Platform;

public interface IEnvironmentManager
{
    void AddUserGlobalPath(string path);
    void AddUserGlobalPathFromHome(string relativeToHome);

    void RemoveUserGlobalPath(string path);

    void AddUserVariable(string key, string value);
    void RemoveUserVariable(string key);
}