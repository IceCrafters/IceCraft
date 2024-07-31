namespace IceCraft.Frontend;

using IceCraft.Core;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

public class RegisterInterceptor : ICommandInterceptor
{
    private readonly IServiceProvider _provider;

    public RegisterInterceptor(IServiceProvider provider)
    {
        _provider = provider;
    }

    void ICommandInterceptor.Intercept(CommandContext context, CommandSettings settings)
    {
        System.Console.WriteLine("Trace: Interceptor run");
        var manager = _provider.GetRequiredService<IRepositorySourceManager>();
        manager.RegisterSourceAsService("adoptium");
    }
}
