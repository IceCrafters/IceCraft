// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

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
        if (_parent.CurrentContext == ExecutionContextType.None 
            ||(_parent.CurrentContext != _contextType &&
               !_contextType.HasFlag(_parent.CurrentContext)))
        {
            throw new SecurityException($"The current API is not allowed in context {_parent.CurrentContext}");
        }
    }

    protected void EnsureContext(ExecutionContextType contextType)
    {
        if (_parent.CurrentContext == ExecutionContextType.None 
            ||(_parent.CurrentContext != contextType &&
               !contextType.HasFlag(_parent.CurrentContext)))
        {
            throw new SecurityException($"The current API is not allowed in context {_parent.CurrentContext}");
        }
    }
}