namespace IceCraft.Core.Platform;

public interface IOutputAdapter
{
    void Error(string message);
    void Error(string format, params object[] args);

    void Warning(string message);
    void Warning(string format, params object[] args);
}
