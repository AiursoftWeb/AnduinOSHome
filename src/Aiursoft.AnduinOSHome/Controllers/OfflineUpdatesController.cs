using Aiursoft.AnduinOSHome.Services;
using Aiursoft.AnduinOSHome.Services.OfflineUpdates;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aiursoft.AnduinOSHome.Controllers;

[AllowAnonymous]
public class OfflineUpdatesController(OfflineCatalogReader catalog) : Controller
{
    [HttpGet("/pro/offline-update-package")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> Index(CancellationToken token)
    {
        var model = await catalog.ReadAsync(token);
        if (!model.Available) Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        return this.SimpleView(model);
    }
}
