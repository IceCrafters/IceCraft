namespace IceCraft.Repositories.Adoptium;

using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using IceCraft.Repositories.Adoptium.Models;
using Microsoft.Extensions.Logging;

internal class AdoptiumApiClient
{
    private readonly ILogger _logger;

    #region Site Service
    private static readonly string Root = "https://api.adoptium.net";

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

    public static string GetApiArchitecture(Architecture architecture)
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

    internal async Task<IEnumerable<AdoptiumBinaryAssetView>?> GetLatestReleaseAsync(int featureVersion, string jvm, Architecture architecture, string imageType)
    {
        var endpoint = $"/v3/assets/latest/{featureVersion}/{jvm}?vendor=eclipse&image_type={imageType}&architecture={GetApiArchitecture(architecture)}";

        return await _client.GetFromJsonAsync<IEnumerable<AdoptiumBinaryAssetView>>($"{Root}{endpoint}]", SerializerOptions);
    }
}