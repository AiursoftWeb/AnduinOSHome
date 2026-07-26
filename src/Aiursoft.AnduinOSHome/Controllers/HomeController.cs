using Aiursoft.AnduinOSHome.Models.HomeViewModels;
using Aiursoft.AnduinOSHome.Services;
using Aiursoft.WebTools.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace Aiursoft.AnduinOSHome.Controllers;

[LimitPerMin]
public class HomeController : Controller
{
    private readonly IConfiguration _configuration;

    public HomeController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public IActionResult Index()
    {
        return this.SimpleView(new IndexViewModel());
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
    public IActionResult ThankYou([FromQuery] string download)
    {
        var links = _configuration.GetSection("DownloadLinks").Get<Dictionary<string, string>>();
        if (links == null || string.IsNullOrEmpty(download) || !links.ContainsKey(download))
        {
            return NotFound();
        }

        var model = new ThankYouViewModel
        {
            DownloadUrl = links[download]
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
