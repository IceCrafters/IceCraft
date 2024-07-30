namespace IceCraft.Repositories.Adoptium;

using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;
using IceCraft.Repositories.Adoptium.Models;

internal class AdoptiumApiClient
{
    #region Site Service
    private static readonly string Root = "https://api.adoptium.net";

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

    internal async Task<AvailableReleaseInfo?> GetAvailableReleases()
    {
        const string endpoint = "/v3/info/available_releases";

        return await _client.GetFromJsonAsync<AvailableReleaseInfo>($"{Root}{endpoint}", SerializerOptions);
    }

    internal async Task<IEnumerable<AdoptiumBinaryAsset>?> GetLatestRelease(int featureVersion, string jvm, string architecture, string imageType)
    {
        var endpoint = $"/v3/assets/latest/{featureVersion}/{jvm}?vendor=eclipse&image_type={imageType}&architecture={architecture}";

        return await _client.GetFromJsonAsync<IEnumerable<AdoptiumBinaryAsset>>($"{Root}{endpoint}]", SerializerOptions);
    }
}