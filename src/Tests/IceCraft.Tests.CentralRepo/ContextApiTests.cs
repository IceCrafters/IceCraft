// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

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

        internal void EnsureContextWrapper(ExecutionContextType contextType)
        {
            EnsureContext(contextType);
        }
    }
    
    [Fact]
    public void ContextApi_DoContext()
    {
        // Arrange
        var parent = new ContextApiRoot();

        // Act
        var contextType = ExecutionContextType.None;
        parent.DoContext(ExecutionContextType.Installation, () => contextType = parent.CurrentContext);
        
        // Assert
        Assert.Equal(ExecutionContextType.Installation, contextType);
    }
    
    [Fact]
    public async Task ContextApi_DoContextAsync()
    {
        // Arrange
        var parent = new ContextApiRoot();

        // Act
        var contextType = ExecutionContextType.None;
        await parent.DoContextAsync(ExecutionContextType.Installation, () => Task.FromResult(contextType = parent.CurrentContext));
        
        // Assert
        Assert.Equal(ExecutionContextType.Installation, contextType);
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
    public void ContextApi_IncludesContext()
    {
        // Arrange
        var parent = new ContextApiRoot
        {
            CurrentContext = ExecutionContextType.Installation
        };

        var api = new MockContextApi(ExecutionContextType.Configuration | ExecutionContextType.Installation, parent);
        
        // Act
        var exception = Record.Exception(() => api.EnsureContextWrapper());
        
        // Assert
        Assert.Null(exception);
    }
    
    [Fact]
    public void ContextApi_NoneContext()
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
    
    [Fact]
    public void ContextApi_InvalidContext()
    {
        // Arrange
        var parent = new ContextApiRoot
        {
            CurrentContext = ExecutionContextType.Metadata
        };

        var api = new MockContextApi(ExecutionContextType.Installation, parent);
        
        // Act
        var exception = Record.Exception(() => api.EnsureContextWrapper());
        
        // Assert
        Assert.IsType<SecurityException>(exception);
    }

    [Fact]
    public void ContextApi_NoneContext_Specified()
    {
        // Arrange
        var parent = new ContextApiRoot
        {
            CurrentContext = ExecutionContextType.None
        };

        var api = new MockContextApi(ExecutionContextType.Installation, parent);
        
        // Act
        var exception = Record.Exception(() => api.EnsureContextWrapper(ExecutionContextType.None));
        
        // Assert
        Assert.IsType<SecurityException>(exception);
    }
    

    [Fact]
    public void ContextApi_InvalidContext_Specified()
    {
        // Arrange
        var parent = new ContextApiRoot
        {
            CurrentContext = ExecutionContextType.Metadata
        };

        var api = new MockContextApi(ExecutionContextType.Installation, parent);
        
        // Act
        var exception = Record.Exception(() => api.EnsureContextWrapper(ExecutionContextType.Configuration));
        
        // Assert
        Assert.IsType<SecurityException>(exception);
    }
}