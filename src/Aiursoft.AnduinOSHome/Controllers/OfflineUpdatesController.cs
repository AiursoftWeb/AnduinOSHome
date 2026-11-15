using Aiursoft.AnduinOSHome.Models.OfflineUpdates;
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
        var result = await catalog.ReadAsync(token);
        if (!result.Available) Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        return this.SimpleView(new OfflineUpdatesViewModel { Available = result.Available, Releases = result.Releases });
    }
}
