namespace IceCraft.Tests.CentralRepo;

using System.Security;
using IceCraft.Extensions.CentralRepo.Runtime.Security;

public class ContextApiTests
{
    private class MockContextApi : ContextApi
    {
        public MockContextApi(ExecutionContextType contextType, ContextApiRoot parent) : base(contextType, parent)
        {
        }

        internal void EnsureContextWrapper()
        {
            EnsureContext();
        }
    }
    
    [Fact]
    public void ContextApi_SameContext()
    {
        // Arrange
        var parent = new ContextApiRoot
        {
            CurrentContext = ExecutionContextType.Configuration
        };

        var api = new MockContextApi(ExecutionContextType.Configuration, parent);
        
        // Act
        var exception = Record.Exception(() => api.EnsureContextWrapper());
        
        // Assert
        Assert.Null(exception);
    }
    
    [Fact]
    public void ContextApi_InvalidContext()
    {
        // Arrange
        var parent = new ContextApiRoot
        {
            CurrentContext = ExecutionContextType.None
        };

        var api = new MockContextApi(ExecutionContextType.Installation, parent);
        
        // Act
        var exception = Record.Exception(() => api.EnsureContextWrapper());
        
        // Assert
        Assert.IsType<SecurityException>(exception);
    }
}