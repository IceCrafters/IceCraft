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
    private static readonly string DefaultUserRootDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "IceCraft");
    private static string? CurrentUserRoot;

    internal static string? UserDataOverride { get; set; }

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

    public string DataBasePath => GetActiveUserRoot();

    public IOutputAdapter Output => Frontend.Output.Shared;

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
        Directory.CreateDirectory(DefaultUserRootDirectory);
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

    public async Task DoStatusTaskAsync(string initialStatus, Func<IStatusReporter, Task> action)
    {
        await AnsiConsole.Status()
            .StartAsync(initialStatus, async ctx => await action(new SpectreStatusReporter(ctx)));
    }

    private static string GetActiveUserRoot()
    {
        if (CurrentUserRoot != null)
        {
            return CurrentUserRoot;
        }

        var envVar = Environment.GetEnvironmentVariable("ICECRAFT_ROOT");
        string result;

        if (UserDataOverride != null)
        {
            // Fail early if direcory doesn't exist and can't be created
            // This method succeeds on existing directory or when created a directory
            Directory.CreateDirectory(UserDataOverride);

            result = UserDataOverride;
        }
        else if (envVar != null)
        {
            // Fail early if direcory doesn't exist and can't be created
            // This method succeeds on existing directory or when created a directory 
            Directory.CreateDirectory(envVar);

            result = envVar;
        }
        else
        {
            result = DefaultUserRootDirectory;
        }

        #if DEBUG
        Frontend.Output.Shared.Log("Selected data directory '{0}'", result);
        #endif

        CurrentUserRoot = result;
        return result;
    }
}
