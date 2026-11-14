using System.Net;
using System.Net.Http.Headers;
using Aiursoft.AnduinOSHome.Configuration;
using Aiursoft.AnduinOSHome.Controllers;
using Aiursoft.AnduinOSHome.Entities;
using Aiursoft.AnduinOSHome.Models.OfflineUpdates;
using Aiursoft.AnduinOSHome.Services.OfflineUpdates;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Aiursoft.AnduinOSHome.Tests.IntegrationTests;

[TestClass]
public class OfflineUpdatesTests : TestBase
{
    private readonly string _batchPrefix = Guid.NewGuid().ToString("N");
    private string Batch(int index) => _batchPrefix + index.ToString("x32");

    [TestCleanup]
    public async Task RemoveOwnPublications()
    {
        using var scope = Server!.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AnduinOSHomeDbContext>();
        db.OfflineUpdates.RemoveRange(await db.OfflineUpdates.Where(x => x.BatchId.StartsWith(_batchPrefix)).ToListAsync());
        await db.SaveChangesAsync();
    }

    [TestMethod]
    public async Task AnonymousPageWorksBeforeStorageIsConfiguredButPublishingIsClosed()
    {
        var page = await Http.GetStringAsync("/pro/offline-update-package");
        Assert.Contains("Your next update is on its way.", page);
        Assert.DoesNotContain("noindex", page);
        using var denied = await Http.PostAsJsonAsync("/api/offline-updates", Request());
        Assert.AreEqual(HttpStatusCode.Unauthorized, denied.StatusCode);
        using var missing = await Http.GetAsync("/pro/offline-update-package/unknown/download");
        Assert.AreEqual(HttpStatusCode.NotFound, missing.StatusCode);
    }

    [TestMethod]
    public async Task HttpValidationRejectsBadVersionsPathsAndMissingFields()
    {
        GetService<IOptions<OfflineUpdatesOptions>>().Value.PublisherToken = "test-publisher";
        Http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "test-publisher");
        foreach (var invalid in new[]
                 {
                     Request() with { TargetVersion = "2.0" },
                     Request() with { Suite = "../../escape" },
                     Request() with { MinimumVersion = "2.0.9" },
                     Request() with { ReleaseLine = "2.1" },
                     Request() with { Summary = "" },
                     Request() with { Size = 0 },
                     Request() with { CreatedUtc = DateTime.UtcNow.AddDays(1) }
                 })
        {
            using var response = await Http.PostAsJsonAsync("/api/offline-updates", invalid);
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode, await response.Content.ReadAsStringAsync());
        }
    }

    [TestMethod]
    public async Task PublicationIsIdempotentAndCannotRewriteOrReviveAWithdrawnBatch()
    {
        using var scope = Server!.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AnduinOSHomeDbContext>();
        var storage = new Storage();
        var controller = Create(db, storage);
        var request = Request() with { CreatedUtc = Request().CreatedUtc.AddTicks(12345) };
        Assert.IsInstanceOfType<CreatedResult>(await controller.Publish(request, default));
        Assert.IsInstanceOfType<OkObjectResult>(await controller.Publish(request, default));
        Assert.AreEqual(1, await db.OfflineUpdates.CountAsync(x => x.BatchId.StartsWith(_batchPrefix)));
        Assert.IsInstanceOfType<ConflictObjectResult>(await controller.Publish(request with { Summary = "Changed" }, default));
        Assert.IsInstanceOfType<NoContentResult>(await controller.Withdraw(request.BatchId, default));
        Assert.IsInstanceOfType<NotFoundResult>(await controller.Download(request.BatchId, default));
        Assert.IsInstanceOfType<ConflictObjectResult>(await controller.Publish(request, default));
        Assert.AreEqual(1, await db.OfflineUpdates.CountAsync(x => x.BatchId.StartsWith(_batchPrefix)));
    }

    [TestMethod]
    public async Task MissingOrUnavailableObjectNeverBecomesADownload()
    {
        using var scope = Server!.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AnduinOSHomeDbContext>();
        var storage = new Storage { Exists = false };
        var controller = Create(db, storage);
        Assert.IsInstanceOfType<ConflictObjectResult>(await controller.Publish(Request(), default));
        Assert.AreEqual(0, await db.OfflineUpdates.CountAsync(x => x.BatchId.StartsWith(_batchPrefix)));
        storage.Exists = true;
        Assert.IsInstanceOfType<CreatedResult>(await controller.Publish(Request(), default));
        var link = (RedirectResult)await controller.Download(Request().BatchId, default);
        Assert.AreEqual("https://download.example/test.atgz", link.Url);
        storage.Exists = false;
        Assert.AreEqual(503, ((StatusCodeResult)await controller.Download(Request().BatchId, default)).StatusCode);
        storage.Throw = true;
        Assert.AreEqual(503, ((StatusCodeResult)await controller.Download(Request().BatchId, default)).StatusCode);
    }

    [TestMethod]
    public async Task DelayedOldReleasesDoNotReplaceLatestAndOnlyThreePerGroupAreListed()
    {
        using var scope = Server!.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AnduinOSHomeDbContext>();
        var controller = Create(db, new Storage());
        foreach (var patch in new[] { 5, 10, 6, 7 })
            Assert.IsInstanceOfType<CreatedResult>(await controller.Publish(Request() with
            {
                BatchId = Batch(patch), TargetVersion = "2.0." + patch,
                Summary = patch == 10 ? "Security <script>alert(1)</script>" : "Update " + patch
            }, default));
        Assert.IsInstanceOfType<CreatedResult>(await controller.Publish(Request() with
        {
            BatchId = Batch(99), Architecture = "arm64"
        }, default));
        var page = await Http.GetStringAsync("/pro/offline-update-package");
        Assert.Contains("AnduinOS 2.0.10", page);
        Assert.Contains("AnduinOS 2.0.7", page);
        Assert.Contains("AnduinOS 2.0.6", page);
        Assert.DoesNotContain("/" + Batch(5) + "/download", page);
        Assert.IsTrue(page.IndexOf("AnduinOS 2.0.10", StringComparison.Ordinal) < page.IndexOf("AnduinOS 2.0.7", StringComparison.Ordinal));
        Assert.Contains("ARM64", page);
        Assert.Contains("Security &lt;script&gt;alert(1)&lt;/script&gt;", page);
        Assert.DoesNotContain("<script>alert(1)</script>", page);
        Assert.AreEqual(5, await db.OfflineUpdates.CountAsync(x => x.BatchId.StartsWith(_batchPrefix)));
    }

    private PublishUpdate Request() => new()
    {
        BatchId = Batch(0), ReleaseLine = "2.0", Suite = "resolute", Architecture = "amd64",
        TargetVersion = "2.0.5", MinimumVersion = "2.0.0", Size = 1024,
        Sha256 = new string('c', 64), SigningFingerprint = new string('D', 40),
        CreatedUtc = new DateTime(2026, 11, 1, 0, 0, 0, DateTimeKind.Utc), Summary = "Security updates."
    };

    private static OfflineUpdatesController Create(AnduinOSHomeDbContext db, Storage storage)
    {
        var controller = new OfflineUpdatesController(db, storage,
            Options.Create(new OfflineUpdatesOptions { PublisherToken = "test-publisher" }),
            NullLogger<OfflineUpdatesController>.Instance)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
        controller.Request.Headers.Authorization = "Bearer test-publisher";
        return controller;
    }

    private sealed class Storage : IOfflineUpdateStorage
    {
        public bool Exists { get; set; } = true;
        public bool Throw { get; set; }
        public Task<bool> MatchesAsync(OfflineUpdate update, CancellationToken token) =>
            Throw ? throw new HttpRequestException("Storage unavailable") : Task.FromResult(Exists);
        public Task<string> GetDownloadUrlAsync(OfflineUpdate update, CancellationToken token) =>
            Task.FromResult("https://download.example/test.atgz");
    }
}
