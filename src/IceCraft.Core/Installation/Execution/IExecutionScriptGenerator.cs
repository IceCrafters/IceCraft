namespace IceCraft.Core.Installation.Execution;

public interface IExecutionScriptGenerator
{
    Task WriteExecutionScriptAsync(ExecutableEntry entry, string executablePath, Stream stream);
}
