namespace IceCraft.Core.Installation.Execution;

public interface IExecutionScriptGenerator
{
    Task WriteExecutionScriptAsync(ExecutableRegistrationEntry entry, string executablePath, Stream stream);
}
