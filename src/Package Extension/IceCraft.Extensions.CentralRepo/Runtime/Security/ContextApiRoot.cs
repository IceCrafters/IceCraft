namespace IceCraft.Extensions.CentralRepo.Runtime.Security;

public class ContextApiRoot
{
    public ExecutionContextType CurrentContext { get; set; }

    public void DoContext(ExecutionContextType context, Action action)
    {
        var old = CurrentContext;

        action();
        
        CurrentContext = old;
    }
}