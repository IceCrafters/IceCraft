namespace IceCraft.Tests;

using System.Text;
using IceCraft.Core.Installation.Execution;
using IceCraft.Core.Platform;

public class ExecutionScriptTests
{
    private static readonly ExecutableRegistrationEntry DummyEntry_NoArgs = new()
    {
        LinkTarget = "test",
        PackageRef = "test"
    };

    [Fact]
    public async Task Posix_NoArgs()
    {
        const string FilePath = "/icMock/packages/test/test";

        // Arrange
        var generator = new PosixExecutionScriptGenerator();

        // Act
        var stream = new MemoryStream();
        await generator.WriteExecutionScriptAsync(DummyEntry_NoArgs, FilePath, stream);
        stream.Close();

        // Assert
        var array = stream.ToArray();
        System.Console.WriteLine(array.Length);
        var str = Encoding.UTF8.GetString(stream.ToArray());
        Assert.Equal($"""
        #!/bin/sh
        # Generated by IceCraft
        '{FilePath}' $@
        
        """.Replace("\r\n", "\n"), str);
    }
}
