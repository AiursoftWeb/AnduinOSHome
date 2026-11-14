using System.Security.Cryptography;
using System.Text;
using Aiursoft.AnduinOSHome.Configuration;
using Aiursoft.AnduinOSHome.Entities;
using Aiursoft.AnduinOSHome.Models.OfflineUpdates;
using Aiursoft.AnduinOSHome.Services;
using Aiursoft.AnduinOSHome.Services.OfflineUpdates;
using Amazon.S3;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Aiursoft.AnduinOSHome.Controllers;

[AllowAnonymous]
public class OfflineUpdatesController(AnduinOSHomeDbContext db, IOfflineUpdateStorage storage,
    IOptions<OfflineUpdatesOptions> options, ILogger<OfflineUpdatesController> logger) : Controller
{
    [HttpGet("/pro/offline-update-package")]
    public async Task<IActionResult> Index(CancellationToken token)
    {
        var updates = await db.OfflineUpdates.AsNoTracking().Where(x => !x.Withdrawn)
            .OrderByDescending(x => x.TargetPatch).ThenByDescending(x => x.CreatedUtc)
            .ThenByDescending(x => x.BatchId).ToListAsync(token);
        return this.SimpleView(new OfflineUpdatesViewModel
        {
            Updates = updates.GroupBy(x => (x.ReleaseLine, x.Suite, x.Architecture))
                .OrderByDescending(g => Version.Parse(g.Key.ReleaseLine))
                .ThenBy(g => g.Key.Architecture, StringComparer.Ordinal).SelectMany(g => g.Take(3)).ToList()
        });
    }

    [HttpGet("/pro/offline-update-package/{batchId}/download")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> Download(string batchId, CancellationToken token)
    {
        var update = await db.OfflineUpdates.AsNoTracking()
            .SingleOrDefaultAsync(x => x.BatchId == batchId && !x.Withdrawn, token);
        if (update == null) return NotFound();
        try
        {
            if (!await storage.MatchesAsync(update, token)) return StatusCode(503);
            return Redirect(await storage.GetDownloadUrlAsync(update, token));
        }
        catch (Exception error) when (error is AmazonS3Exception or InvalidOperationException or HttpRequestException)
        {
            logger.LogWarning("Offline download unavailable for batch {Batch}: {ErrorType}", batchId, error.GetType().Name);
            return StatusCode(503);
        }
    }

    [HttpPost("/api/offline-updates")]
    [RequestSizeLimit(16384)]
    public async Task<IActionResult> Publish([FromBody] PublishUpdate? request, CancellationToken token)
    {
        if (!AuthorizedPublisher()) return Unauthorized();
        if (request == null || !ModelState.IsValid) return BadRequest(ModelState);
        if (!Version.TryParse(request.TargetVersion, out var target) ||
            !Version.TryParse(request.MinimumVersion, out var minimum) || minimum > target ||
            $"{target.Major}.{target.Minor}" != request.ReleaseLine ||
            $"{minimum.Major}.{minimum.Minor}" != request.ReleaseLine ||
            request.CreatedUtc.Kind != DateTimeKind.Utc || request.CreatedUtc.Year < 2026 ||
            request.CreatedUtc > DateTime.UtcNow.AddMinutes(5)) return BadRequest();
        var update = new OfflineUpdate
        {
            BatchId = request.BatchId, ReleaseLine = request.ReleaseLine, Suite = request.Suite,
            Architecture = request.Architecture, TargetVersion = request.TargetVersion, TargetPatch = target.Build,
            MinimumVersion = request.MinimumVersion, Sha256 = request.Sha256, Size = request.Size,
            SigningFingerprint = request.SigningFingerprint,
            // MySQL stores fewer fractional digits than .NET. Canonicalize for identical retries.
            CreatedUtc = new DateTime(request.CreatedUtc.Ticks - request.CreatedUtc.Ticks % TimeSpan.TicksPerMillisecond, DateTimeKind.Utc),
            PublishedUtc = DateTime.UtcNow, Summary = request.Summary
        };
        var previous = await db.OfflineUpdates.AsNoTracking().SingleOrDefaultAsync(x => x.BatchId == update.BatchId, token);
        if (previous != null) return Existing(previous, update);
        try
        {
            if (!await storage.MatchesAsync(update, token)) return Conflict("Upload and verify the object before publishing.");
        }
        catch (Exception error) when (error is AmazonS3Exception or InvalidOperationException or HttpRequestException)
        {
            logger.LogWarning("Offline publication storage unavailable: {ErrorType}", error.GetType().Name);
            return StatusCode(503);
        }
        db.OfflineUpdates.Add(update);
        try { await db.SaveChangesAsync(token); }
        catch (DbUpdateException)
        {
            db.Entry(update).State = EntityState.Detached;
            previous = await db.OfflineUpdates.AsNoTracking().SingleOrDefaultAsync(x => x.BatchId == update.BatchId, token);
            if (previous == null) throw;
            return Existing(previous, update);
        }
        return Created($"/pro/offline-update-package/{update.BatchId}/download", new { update.BatchId });
    }

    [HttpDelete("/api/offline-updates/{batchId}")]
    public async Task<IActionResult> Withdraw(string batchId, CancellationToken token)
    {
        if (!AuthorizedPublisher()) return Unauthorized();
        var update = await db.OfflineUpdates.SingleOrDefaultAsync(x => x.BatchId == batchId, token);
        if (update == null) return NotFound();
        update.Withdrawn = true;
        await db.SaveChangesAsync(token);
        return NoContent();
    }

    private IActionResult Existing(OfflineUpdate previous, OfflineUpdate requested) =>
        !previous.Withdrawn && previous.Sha256 == requested.Sha256 && previous.Size == requested.Size &&
        previous.ReleaseLine == requested.ReleaseLine && previous.Suite == requested.Suite &&
        previous.Architecture == requested.Architecture && previous.TargetVersion == requested.TargetVersion &&
        previous.MinimumVersion == requested.MinimumVersion && previous.CreatedUtc == requested.CreatedUtc &&
        previous.SigningFingerprint == requested.SigningFingerprint && previous.Summary == requested.Summary
            ? Ok(new { previous.BatchId }) : Conflict("This batch already exists with different metadata or was withdrawn.");

    private bool AuthorizedPublisher()
    {
        var expected = options.Value.PublisherToken;
        if (string.IsNullOrWhiteSpace(expected)) return false;
        var actual = Request.Headers.Authorization.ToString();
        return CryptographicOperations.FixedTimeEquals(SHA256.HashData(Encoding.UTF8.GetBytes(actual)),
            SHA256.HashData(Encoding.UTF8.GetBytes("Bearer " + expected)));
    }
}
