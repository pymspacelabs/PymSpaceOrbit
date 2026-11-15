using System.Collections.Concurrent;
using System.Text.Json;
using PymSpaceOrbit.Shared.Abstractions.ML;

namespace PymSpaceOrbit.Shared.Services;

/// <summary>
/// Lightweight in-memory cached NASA Image API resolver that queries images-api.nasa.gov
/// and falls back immediately to NasaImageAssetUrl.
/// </summary>
public sealed class NasaCardImageResolver
{
    private readonly HttpClient _httpClient;
    private readonly IPymGameOrbitMlModelService _mlModelService;
    private readonly ConcurrentDictionary<string, string> _cache = new();

    public NasaCardImageResolver(HttpClient httpClient, IPymGameOrbitMlModelService mlModelService)
    {
        _httpClient = httpClient;
        _mlModelService = mlModelService;
    }

    public async Task<string> ResolveImageUrlAsync(
        string? nasaImageApiUrl,
        string? nasaImageAssetUrl,
        CancellationToken cancellationToken = default)
    {
        string fallback = nasaImageAssetUrl ?? string.Empty;

        if (string.IsNullOrWhiteSpace(nasaImageApiUrl) || !_mlModelService.LiveNasaApiSyncEnabled)
        {
            return fallback;
        }

        if (_cache.TryGetValue(nasaImageApiUrl, out var cachedUrl) && !string.IsNullOrWhiteSpace(cachedUrl))
        {
            return cachedUrl;
        }

        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(3.5));

            using var response = await _httpClient.GetAsync(nasaImageApiUrl, HttpCompletionOption.ResponseHeadersRead, cts.Token);
            if (response.IsSuccessStatusCode)
            {
                var jsonStream = await response.Content.ReadAsStreamAsync(cts.Token);
                using var doc = await JsonDocument.ParseAsync(jsonStream, cancellationToken: cts.Token);

                if (doc.RootElement.TryGetProperty("collection", out var collection) &&
                    collection.TryGetProperty("items", out var items) &&
                    items.GetArrayLength() > 0)
                {
                    var firstItem = items[0];
                    if (firstItem.TryGetProperty("links", out var links) && links.GetArrayLength() > 0)
                    {
                        var firstLink = links[0];
                        if (firstLink.TryGetProperty("href", out var hrefProp))
                        {
                            var resolved = hrefProp.GetString();
                            if (!string.IsNullOrWhiteSpace(resolved))
                            {
                                _cache[nasaImageApiUrl] = resolved;
                                return resolved;
                            }
                        }
                    }
                }
            }
        }
        catch
        {
            // Network failure, offline mode, timeout or rate-limit -> safely fallback
        }

        _cache[nasaImageApiUrl] = fallback;
        return fallback;
    }
}
