namespace IceCraft.Api.Platform;

public interface IExecutionScriptGenerator
{
    Task WriteExecutionScriptAsync(ExecutableRegistrationEntry entry, string executablePath, Stream stream);
}
