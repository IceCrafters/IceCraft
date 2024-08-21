namespace IceCraft;

using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using IceCraft.Core.Network;
using IceCraft.Core.Platform;
using IceCraft.Frontend;
using Serilog;
using Spectre.Console;

internal class IceCraftApp : IFrontendApp
{
    private static readonly string UserDataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "IceCraft");
    internal static readonly string CachesDirectory = Path.Combine(UserDataDirectory, "caches");
    
    private static readonly CancellationTokenSource _tokenSource = new();

    private static readonly HttpClient HttpClient = new()
    {
        DefaultRequestHeaders =
        {
            UserAgent =
            {
                new ProductInfoHeaderValue("IceCraft", ProductVersion)
            }
        },
        Timeout = new TimeSpan(0, 0, 20)
    };

    /// <summary>
    /// Gets the product version information of the IceCraft driver.
    /// </summary>
    /// <value>
    /// The <see cref="FileVersionInfo.ProductVersion"/> property acquired from the assembly file, 
    /// or <see langword="null"/> if assembly file is not found or does not contain version information.
    /// </value>
    internal static readonly string ProductVersion = GetProductVersion();

    public string ProductName => "IceCraft";

    string IFrontendApp.ProductVersion => ProductVersion;

    public string DataBasePath => UserDataDirectory;

    private static string GetProductVersion()
    {
        var assemblyFile = Assembly.GetExecutingAssembly().Location;
        if (!File.Exists(assemblyFile))
        {
            return "<unknown>";
        }

        var versionInfo = FileVersionInfo.GetVersionInfo(assemblyFile);
        return versionInfo.ProductVersion ?? "<unknown>";
    }

    public static void Initialize()
    {
        Directory.CreateDirectory(UserDataDirectory);
        Console.CancelKeyPress += Console_CancelKeyPress;
    }

    private static void Console_CancelKeyPress(object? sender, ConsoleCancelEventArgs e)
    {
        Console.WriteLine();
        Console.WriteLine("-----------------------");
        Console.WriteLine();
        Log.Warning("Cancelled");
        _tokenSource.Cancel();
    }

    public HttpClient GetClient() => HttpClient;

    public CancellationToken GetCancellationToken()
    {
        return _tokenSource.Token;
    }

    public async Task DoProgressedTaskAsync(string description, Func<IProgressedTask, Task> action)
    {
        var progress = AnsiConsole.Progress();
        await progress.StartAsync(async (context) =>
        {
            var task = context.AddTask(description);
            await action.Invoke(new SpectreProgressedTask(task));
        });
    }

    [Obsolete("Use DoProgressedTaskAsync instead.")]
    public async Task DoDownloadTaskAsync(Func<INetworkDownloadTask, Task> action)
    {
         var progress = AnsiConsole.Progress();
        await progress.StartAsync(async (context) =>
        {
            var task = context.AddTask("Download");
            await action.Invoke(new SpectreDownloadTask(task));
        });
    }
}
