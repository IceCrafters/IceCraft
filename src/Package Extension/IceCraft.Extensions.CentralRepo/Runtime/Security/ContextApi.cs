namespace IceCraft.Extensions.CentralRepo.Runtime.Security;

using System.Security;

/// <summary>
/// Provides a base class for context-dependent APIs.
/// </summary>
public abstract class ContextApi
{
    private readonly ContextApiRoot _parent;
    private readonly ExecutionContextType _contextType;
    
    protected ContextApi(ExecutionContextType contextType, ContextApiRoot parent)
    {
        _contextType = contextType;
        _parent = parent;
    }

    protected void EnsureContext()
    {
        if (_parent.CurrentContext != _contextType)
        {
            throw new SecurityException($"The current API is not allowed in context {_parent.CurrentContext}");
        }
    }
}