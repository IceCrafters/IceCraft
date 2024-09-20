// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Extensions.CentralRepo.Runtime.Security;

public class ContextApiRoot
{
    public ExecutionContextType CurrentContext { get; set; }

    public void DoContext(ExecutionContextType context, Action action)
    {
        var old = CurrentContext;
        CurrentContext = context;

        action();
        
        CurrentContext = old;
    }
    
    public async Task DoContextAsync(ExecutionContextType context, Func<Task> action)
    {
        var old = CurrentContext;
        CurrentContext = context;

        await action();
        
        CurrentContext = old;
    }
}