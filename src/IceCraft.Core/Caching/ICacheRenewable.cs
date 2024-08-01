namespace IceCraft.Core.Caching;

public interface ICacheRenewable
{
    Task RegenerateCache();
}
