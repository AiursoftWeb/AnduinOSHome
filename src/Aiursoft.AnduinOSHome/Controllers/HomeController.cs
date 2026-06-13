using Aiursoft.AnduinOSHome.Models.HomeViewModels;
using Aiursoft.AnduinOSHome.Services;
using Aiursoft.WebTools.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace Aiursoft.AnduinOSHome.Controllers;

[LimitPerMin]
public class HomeController : Controller
{
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
    public IActionResult ThankYou()
    {
        return this.SimpleView(new ThankYouViewModel());
    }

}
