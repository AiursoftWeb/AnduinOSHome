using Aiursoft.AnduinOSHome.Services;
using Microsoft.AspNetCore.Mvc;

namespace Aiursoft.AnduinOSHome.Views.Shared.Components.RecommendedHardware;

public class RecommendedHardware(HardwareCatalog catalog) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync() => View(await catalog.PublishedAsync(true));
}
