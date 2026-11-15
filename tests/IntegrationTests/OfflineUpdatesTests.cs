using System.Net;
using System.Text.Json;
using Aiursoft.AnduinOSHome.Models.OfflineUpdates;
using Aiursoft.AnduinOSHome.Services.OfflineUpdates;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;

namespace Aiursoft.AnduinOSHome.Tests.IntegrationTests;

[TestClass]
public class OfflineUpdatesTests : TestBase
{
    [TestMethod]
    public async Task AnonymousPageLinksToR2WithoutPublisherApi()
    {
        var cache = Server!.Services.GetRequiredService<IMemoryCache>();
        var release = Release(5, 1);
        cache.Set(OfflineCatalogReader.CatalogUrl, new OfflineCatalogResult(true, [release]));
        var response = await Http.GetAsync("/pro/offline-update-package");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains(release.DownloadUrl, html);
        Assert.Contains("Intel / AMD (x64)", html);
        Assert.Contains("2.0.5", html);
        Assert.Contains("no-store", response.Headers.CacheControl!.ToString());
        using var chinese = await Http.GetAsync("/pro/offline-update-package?culture=zh-CN&ui-culture=zh-CN");
        Assert.Contains("离线更新", WebUtility.HtmlDecode(await chinese.Content.ReadAsStringAsync()));
        using var englishAgain = await Http.GetAsync("/pro/offline-update-package?culture=en-US&ui-culture=en-US");
        Assert.Contains("Offline updates", await englishAgain.Content.ReadAsStringAsync());
        using var publication = await Http.PostAsync("/api/offline-updates", new StringContent("{}"));
        Assert.AreEqual(HttpStatusCode.NotFound, publication.StatusCode);
    }

    [TestMethod]
    public async Task UnavailableAndEmptyCatalogHaveDifferentMessages()
    {
        var cache = Server!.Services.GetRequiredService<IMemoryCache>();
        cache.Set(OfflineCatalogReader.CatalogUrl, new OfflineCatalogResult(false, []));
        using var unavailable = await Http.GetAsync("/pro/offline-update-package");
        Assert.AreEqual(HttpStatusCode.ServiceUnavailable, unavailable.StatusCode);
        Assert.Contains("Downloads are temporarily unavailable.", await unavailable.Content.ReadAsStringAsync());
        cache.Set(OfflineCatalogReader.CatalogUrl, new OfflineCatalogResult(true, []));
        using var empty = await Http.GetAsync("/pro/offline-update-package");
        Assert.AreEqual(HttpStatusCode.OK, empty.StatusCode);
        Assert.Contains("Your next update is on its way.", await empty.Content.ReadAsStringAsync());
    }

    internal static OfflineRelease Release(int patch, int batch, string architecture = "amd64")
    {
        var id = batch.ToString("x64");
        return new OfflineRelease
        {
            Update = new OfflinePackage
            {
                BatchId = id, ReleaseLine = "2.0", Suite = "resolute", Architecture = architecture,
                TargetVersion = $"2.0.{patch}", MinimumVersion = "2.0.0", Sha256 = new string('a', 64),
                Size = 2_000_000_000, CreatedUtc = DateTimeOffset.Parse("2026-11-15T08:00:00Z").AddHours(batch)
            },
            PublishedUtc = DateTimeOffset.Parse("2026-11-15T09:00:00Z").AddHours(batch),
            ObjectKey = $"offline/resolute/2.0/{architecture}/{id}/{new string('a', 64)}.atgz"
        };
    }
}

[TestClass]
public class OfflineCatalogReaderTests
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };

    [TestMethod]
    public async Task OrdersNumericVersionsAndRetainsThreePerGroupIncludingSameVersionBatches()
    {
        var releases = new[]
        {
            OfflineUpdatesTests.Release(9, 1), OfflineUpdatesTests.Release(10, 2),
            OfflineUpdatesTests.Release(10, 3), OfflineUpdatesTests.Release(10, 4),
            OfflineUpdatesTests.Release(10, 5, "arm64")
        };
        var json = JsonSerializer.Serialize(new { schema_version = 1, releases, retired = new[] { releases[0] } }, JsonOptions);
        using var handler = new CatalogHandler(json);
        using var http = new HttpClient(handler);
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var reader = new OfflineCatalogReader(http, cache, NullLogger<OfflineCatalogReader>.Instance);
        var result = await reader.ReadAsync(CancellationToken.None);
        Assert.IsTrue(result.Available);
        Assert.AreEqual(4, result.Releases.Count);
        CollectionAssert.AreEqual(new[] { 4, 3, 2, 5 }.Select(x => x.ToString("x64")).ToArray(),
            result.Releases.Select(x => x.Update.BatchId).ToArray());
        await reader.ReadAsync(CancellationToken.None);
        Assert.AreEqual(1, handler.Calls);
        cache.Remove(OfflineCatalogReader.CatalogUrl);
        handler.Body = "{\"schema_version\":1,\"releases\":[],\"retired\":[]}";
        Assert.AreEqual(0, (await reader.ReadAsync(CancellationToken.None)).Releases.Count);
        Assert.AreEqual(2, handler.Calls);
    }

    [TestMethod]
    public async Task InvalidCatalogDoesNotProduceDownloadLinks()
    {
        var good = JsonSerializer.Serialize(new { schema_version = 1, releases = new[] { OfflineUpdatesTests.Release(5, 1) } }, JsonOptions);
        string[] invalid =
        [
            "not json", "{\"schema_version\":2,\"releases\":[]}", "{\"schema_version\":1}",
            "{\"schema_version\":1,\"releases\":[null]}",
            good.Replace("offline/resolute", "https://elsewhere.invalid/offline/resolute"),
            good.Replace("offline/resolute", "../offline/resolute"),
            good.Replace("2.0.0", "2.0.9"), good.Replace("amd64", "unknown"),
            good.Replace("2000000000", "0")
        ];
        foreach (var json in invalid)
        {
            using var handler = new CatalogHandler(json);
            using var http = new HttpClient(handler);
            using var cache = new MemoryCache(new MemoryCacheOptions());
            var result = await new OfflineCatalogReader(http, cache, NullLogger<OfflineCatalogReader>.Instance).ReadAsync(CancellationToken.None);
            Assert.IsFalse(result.Available, json);
            Assert.AreEqual(0, result.Releases.Count);
        }
    }

    [TestMethod]
    public async Task UpstreamErrorDoesNotPretendNoPackagesExist()
    {
        using var handler = new CatalogHandler("{}", HttpStatusCode.ServiceUnavailable);
        using var http = new HttpClient(handler);
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var reader = new OfflineCatalogReader(http, cache, NullLogger<OfflineCatalogReader>.Instance);
        Assert.IsFalse((await reader.ReadAsync(CancellationToken.None)).Available);
    }

    private sealed class CatalogHandler(string body, HttpStatusCode status = HttpStatusCode.OK) : HttpMessageHandler
    {
        public string Body { get; set; } = body;
        public int Calls { get; private set; }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Assert.AreEqual(OfflineCatalogReader.CatalogUrl, request.RequestUri!.ToString());
            Calls++;
            return Task.FromResult(new HttpResponseMessage(status) { Content = new StringContent(Body) });
        }
    }
}
