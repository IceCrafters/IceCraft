namespace IceCraft.Repositories.Adoptium;

using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text.Json;
using Flurl;
using IceCraft.Repositories.Adoptium.Models;
using Microsoft.Extensions.Logging;

internal class AdoptiumApiClient
{
    private readonly ILogger _logger;

    #region Site Service

    private const string Root = "https://api.adoptium.net";

    public static bool IsArchitectureSupported(Architecture architecture)
    {
        return architecture switch
        {
            Architecture.X86 or
            Architecture.X64 or
            Architecture.Arm or
            Architecture.Arm64 or
            Architecture.Armv6 => true,
            _ => false
        };
    }

    [SupportedOSPlatformGuard("linux")]
    [SupportedOSPlatformGuard("windows")]
    [SupportedOSPlatformGuard("macos")]
    public static bool IsOsSupported()
    {
        return OperatingSystem.IsLinux()
               || OperatingSystem.IsWindows()
               || OperatingSystem.IsMacOS();
    }

    private static string GetApiArchitecture(Architecture architecture)
    {
        return architecture switch
        {
            Architecture.X86 => "x86",
            Architecture.X64 => "x64",
            Architecture.Arm or Architecture.Armv6 => "arm",
            Architecture.Arm64 => "arm64",
            _ => throw new ArgumentException("Unsupported architecture.", nameof(architecture))
        };
    }

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    private readonly HttpClient _client = new()
    {
        DefaultRequestHeaders =
        {
            UserAgent =
            {
                new ProductInfoHeaderValue(ClientMeta, ClientVersion)
            }
        }
    };
    #endregion

    #region Client Information
    private const string ClientMeta = "IceCraft";

    private static readonly string ClientVersion = Assembly.GetExecutingAssembly()
        .GetName()
        .Version?.ToString() ?? "unknown";

    internal static string GetOs()
    {
        if (OperatingSystem.IsLinux())
        {
            return "linux";
        }
        
        if (OperatingSystem.IsWindows())
        {
            return "windows";
        }

        if (OperatingSystem.IsMacOS())
        {
            return "mac";
        }

        throw new PlatformNotSupportedException("Platform not supported.");
    }
    #endregion

    public AdoptiumApiClient(ILogger logger)
    {
        _logger = logger;
    }

    internal async Task<AvailableReleaseInfo?> GetAvailableReleasesAsync()
    {
        const string endpoint = "/v3/info/available_releases";

        return await _client.GetFromJsonAsync<AvailableReleaseInfo>($"{Root}{endpoint}", SerializerOptions);
    }

    internal async Task<IEnumerable<AdoptiumBinaryAssetView>?> GetLatestReleaseAsync(int featureVersion, 
        string jvm, 
        Architecture architecture, 
        string imageType,
        string os)
    {
        var url = new Url(Root)
            .AppendPathSegment("/v3/assets/latest")
            .AppendPathSegment(featureVersion)
            .AppendPathSegment(jvm)
            .AppendQueryParam("vendor", "eclipse")
            .AppendQueryParam("image_type", imageType)
            .AppendQueryParam("architecture", GetApiArchitecture(architecture))
            .AppendQueryParam("os", os);

        return await _client.GetFromJsonAsync<IEnumerable<AdoptiumBinaryAssetView>>(url, SerializerOptions);
    }

    internal async Task<IEnumerable<AdoptiumBinaryAssetView>?> GetFeatureReleasesAsync(int featureVersion, 
        string releaseType, 
        Architecture architecture, 
        string imageType,
        string jvm,
        string os)
    {
        _logger.LogTrace("Getting eclipse (Java {FeatureVersion}) {Jvm}/{ImageType} {ReleaseType} releases for os {OS} and arch {Architecture}", 
            featureVersion, 
            jvm, 
            imageType,
            releaseType, 
            os, 
            GetApiArchitecture(architecture));

        var url = new Url(Root)
            .AppendPathSegment("/v3/assets/feature_releases/")
            .AppendPathSegment(featureVersion)
            .AppendPathSegment(releaseType)
            .AppendQueryParam("vendor", "eclipse")
            .AppendQueryParam("image_type", imageType)
            .AppendQueryParam("architecture", GetApiArchitecture(architecture))
            .AppendQueryParam("jvm_impl", jvm)
            .AppendQueryParam("os", os);

        var response = await _client.GetAsync(url);
        _logger.LogDebug("Url: {Url}", url);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Found nothing for Java {FeatureVersion} {Jvm}/{ImageType} {ReleaseType} releases for os {OS} and arch {Architecture}", 
            featureVersion, 
            jvm, 
            imageType,
            releaseType, 
            os, 
            GetApiArchitecture(architecture));
            return [];
        }

        return await response.Content.ReadFromJsonAsync<IEnumerable<AdoptiumBinaryAssetView>>(SerializerOptions);
    }
}