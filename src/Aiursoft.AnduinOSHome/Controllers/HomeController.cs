using Aiursoft.AnduinOSHome.Configuration;
using Aiursoft.AnduinOSHome.Models.HomeViewModels;
using Aiursoft.AnduinOSHome.Services;
using Aiursoft.WebTools.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Aiursoft.AnduinOSHome.Controllers;

[LimitPerMin]
public class HomeController : Controller
{
    private const string TorrentSuffix = "-torrent";
    private const string LegacyFastTrackChannel = "fast-track";
    private readonly ReleaseArtifactsOptions _releaseArtifacts;

    public HomeController(IOptions<ReleaseArtifactsOptions> releaseArtifacts)
    {
        _releaseArtifacts = releaseArtifacts.Value;
    }

    public IActionResult Index()
    {
        return this.SimpleView(new IndexViewModel
        {
            ResoluteArtifacts = _releaseArtifacts.Versions[VersionData.Resolute.LatestPatch],
            StonkingArtifacts = _releaseArtifacts.Versions[VersionData.Stonking.LatestPatch]
        });
    }

    [Route("/Compare.html")]
    public IActionResult Compare()
    {
        return this.SimpleView(new CompareViewModel());
    }

    [Route("/pro")]
    public IActionResult Pro()
    {
        return this.SimpleView(new ProViewModel());
    }

    [Route("/privacy.html")]
    public IActionResult Privacy()
    {
        return this.SimpleView(new PrivacyViewModel());
    }

    [Route("/terms.html")]
    public IActionResult Terms()
    {
        return this.SimpleView(new TermsViewModel());
    }

    [Route("/thankyou.html")]
    public IActionResult ThankYou([FromQuery] string? download, [FromQuery] string? version, [FromQuery] string? channel)
    {
        version ??= channel switch
        {
            null or "lts" => VersionData.Resolute.LatestPatch,
            LegacyFastTrackChannel => VersionData.Stonking.LatestPatch,
            _ => null
        };
        if (version is null || !_releaseArtifacts.Versions.TryGetValue(version, out var artifacts) ||
            string.IsNullOrWhiteSpace(download))
        {
            return NotFound();
        }

        var isTorrent = download.EndsWith(TorrentSuffix, StringComparison.OrdinalIgnoreCase);
        var architecture = isTorrent ? download[..^TorrentSuffix.Length] : download;
        if (!artifacts.TryGetValue(architecture, out var artifact))
        {
            return NotFound();
        }

        var model = new ThankYouViewModel
        {
            DownloadUrl = isTorrent ? artifact.TorrentUrl : artifact.IsoUrl,
            IsoUrl = artifact.IsoUrl,
            ChecksumUrl = artifact.ChecksumUrl,
            IsoFileName = artifact.IsoFileName,
            IsoSize = artifact.IsoSize,
            ReleaseVersion = version,
            IsTorrent = isTorrent
        };
        return this.SimpleView(model);
    }

    [Route("/HistoryBuilds.html")]
    public IActionResult HistoryBuilds()
    {
        return this.SimpleView(new HistoryBuildsViewModel());
    }

    [Route("/MigrateFrom1x.html")]
    public IActionResult MigrateFrom1x()
    {
        return this.SimpleView(new MigrateFrom1xViewModel());
    }

    [Route("/Container.html")]
    public IActionResult Container()
    {
        return this.SimpleView(new ContainerViewModel());
    }

}
