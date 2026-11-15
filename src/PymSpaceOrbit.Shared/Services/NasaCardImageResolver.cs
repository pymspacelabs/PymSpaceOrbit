using System.Collections.Concurrent;
using System.Text.Json;
using PymSpaceOrbit.Shared.Abstractions.ML;

namespace PymSpaceOrbit.Shared.Services;

/// <summary>
/// Lightweight in-memory cached NASA Image API resolver that queries images-api.nasa.gov
/// and falls back immediately to verified NasaImageAssetUrl without breaking card rendering.
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

        // If we already have a verified images-assets.nasa.gov URL in the catalog, prefer it immediately
        // and only hit the search API when fallback is empty.
        if (!string.IsNullOrWhiteSpace(fallback) &&
            fallback.StartsWith("https://images-assets.nasa.gov/image/", StringComparison.OrdinalIgnoreCase))
        {
            _cache[nasaImageApiUrl] = fallback;
            return fallback;
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
                    foreach (var item in items.EnumerateArray())
                    {
                        if (!item.TryGetProperty("links", out var links) || links.GetArrayLength() == 0)
                            continue;

                        foreach (var link in links.EnumerateArray())
                        {
                            if (link.TryGetProperty("href", out var hrefProp))
                            {
                                var resolved = hrefProp.GetString();
                                if (!string.IsNullOrWhiteSpace(resolved) &&
                                    (resolved.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                                     resolved.EndsWith(".png", StringComparison.OrdinalIgnoreCase)))
                                {
                                    _cache[nasaImageApiUrl] = resolved;
                                    return resolved;
                                }
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
