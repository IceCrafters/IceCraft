namespace IceCraft.Core.Platform;

public interface IOutputAdapter
{
    void Error(string message);
    void Error(string format, params object?[] args);

    void Warning(string message);
    void Warning(Exception exception, string message);
    void Warning(string format, params object?[] args);

    void Log(string message);
    void Log(string format, params object?[] args);

    void Verbose(string message);
    void Verbose(string format, params object?[] args);
}
