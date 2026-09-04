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
    private readonly ReleaseArtifactsOptions _releaseArtifacts;

    public HomeController(IOptions<ReleaseArtifactsOptions> releaseArtifacts)
    {
        _releaseArtifacts = releaseArtifacts.Value;
    }

    public IActionResult Index()
    {
        return this.SimpleView(new IndexViewModel
        {
            Amd64ChecksumUrl = _releaseArtifacts.Architectures["amd64"].ChecksumUrl,
            Arm64ChecksumUrl = _releaseArtifacts.Architectures["arm64"].ChecksumUrl
        });
    }

    [Route("/Compare.html")]
    public IActionResult Compare()
    {
        return this.SimpleView(new CompareViewModel());
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
    public IActionResult ThankYou([FromQuery] string? download)
    {
        if (string.IsNullOrWhiteSpace(download))
        {
            return NotFound();
        }

        var isTorrent = download.EndsWith(TorrentSuffix, StringComparison.OrdinalIgnoreCase);
        var architecture = isTorrent ? download[..^TorrentSuffix.Length] : download;
        if (!_releaseArtifacts.Architectures.TryGetValue(architecture, out var artifact))
        {
            return NotFound();
        }

        var model = new ThankYouViewModel
        {
            DownloadUrl = isTorrent ? artifact.TorrentUrl : artifact.IsoUrl,
            ChecksumUrl = artifact.ChecksumUrl,
            IsoFileName = artifact.IsoFileName,
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
