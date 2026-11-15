using System.Text.Json;
using System.Text.RegularExpressions;
using Aiursoft.AnduinOSHome.Models.OfflineUpdates;
using Microsoft.Extensions.Caching.Memory;

namespace Aiursoft.AnduinOSHome.Services.OfflineUpdates;

public sealed class OfflineCatalogReader(HttpClient http, IMemoryCache cache, ILogger<OfflineCatalogReader> logger)
{
    public const string CatalogUrl = "https://offline-update-packages.anduinos.com/offline/catalog.json";
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };

    public async Task<OfflineCatalogResult> ReadAsync(CancellationToken token)
    {
        if (cache.TryGetValue<OfflineCatalogResult>(CatalogUrl, out var cached) && cached != null) return cached;
        try
        {
            using var response = await http.GetAsync(CatalogUrl, token);
            response.EnsureSuccessStatusCode();
            var catalog = JsonSerializer.Deserialize<OfflineCatalog>(await response.Content.ReadAsStringAsync(token), JsonOptions);
            if (catalog is not { SchemaVersion: 1, Releases: not null } || catalog.Releases.Any(x => !IsValid(x)))
                throw new JsonException("Unsupported or invalid offline update catalog.");
            var releases = catalog.Releases
                .GroupBy(x => (x.Update.ReleaseLine, x.Update.Suite, x.Update.Architecture))
                .OrderByDescending(g => Version.Parse(g.Key.ReleaseLine)).ThenBy(g => g.Key.Suite, StringComparer.Ordinal)
                .ThenBy(g => g.Key.Architecture, StringComparer.Ordinal)
                .SelectMany(g => g.OrderByDescending(x => Version.Parse(x.Update.TargetVersion))
                    .ThenByDescending(x => x.PublishedUtc).ThenByDescending(x => x.Update.CreatedUtc)
                    .DistinctBy(x => x.Update.BatchId).Take(3)).ToList();
            return cache.Set(CatalogUrl, new OfflineCatalogResult(true, releases), TimeSpan.FromMinutes(5));
        }
        catch (Exception error) when (error is HttpRequestException or JsonException ||
                                      error is OperationCanceledException && !token.IsCancellationRequested)
        {
            logger.LogWarning(error, "Could not read the offline update catalog.");
            return cache.Set(CatalogUrl, new OfflineCatalogResult(false, []), TimeSpan.FromSeconds(15));
        }
    }

    private static bool IsValid(OfflineRelease? release)
    {
        if (release?.Update is not { } update ||
            !Hex(update.BatchId) || !Hex(update.Sha256) ||
            update.Architecture is not ("amd64" or "arm64") ||
            string.IsNullOrEmpty(update.Suite) || !Regex.IsMatch(update.Suite, "^[a-z][a-z0-9-]{0,31}$") ||
            !Version.TryParse(update.TargetVersion, out var target) || target.Build < 0 ||
            !Version.TryParse(update.MinimumVersion, out var minimum) || minimum.Build < 0 || minimum > target ||
            update.ReleaseLine != $"{target.Major}.{target.Minor}" ||
            update.ReleaseLine != $"{minimum.Major}.{minimum.Minor}" ||
            update.Size <= 0 || update.CreatedUtc == default || release.PublishedUtc == default)
            return false;
        // Only immutable objects in the publisher's documented namespace can become download links.
        return release.ObjectKey == $"offline/{update.Suite}/{update.ReleaseLine}/{update.Architecture}/{update.BatchId}/{update.Sha256}.atgz";
    }

    private static bool Hex(string? value) => value is { Length: 64 } && value.All(c => c is >= '0' and <= '9' or >= 'a' and <= 'f');
}
