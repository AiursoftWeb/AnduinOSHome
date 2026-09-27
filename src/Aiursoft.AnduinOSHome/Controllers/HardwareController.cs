using Aiursoft.AnduinOSHome.Entities;
using Aiursoft.AnduinOSHome.Models.HardwareViewModels;
using Aiursoft.AnduinOSHome.Services;
using Aiursoft.WebTools.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.AnduinOSHome.Controllers;

[LimitPerMin]
public class HardwareController(AnduinOSHomeDbContext db, HardwareCatalog catalog) : Controller
{
    public async Task<IActionResult> Index(int page = 1, string? search = null, HardwareDeviceType? deviceType = null,
        string? architecture = null, bool teamOnly = false)
    {
        if (!ModelState.IsValid || page < 1 || page > 100000 || search?.Length > 100 ||
            architecture?.Length > 30 || (deviceType.HasValue && !Enum.IsDefined(deviceType.Value)))
            return BadRequest();
        search = string.IsNullOrWhiteSpace(search) ? null : search.Trim();
        architecture = string.IsNullOrWhiteSpace(architecture) ? null : architecture.Trim();
        var items = await catalog.PublishedAsync(page: page, search: search, deviceType: deviceType,
            architecture: architecture, teamOnly: teamOnly);
        return this.SimpleView(new HardwareIndexModel
        {
            Items = items.Take(24).ToList(), Page = page, HasNextPage = items.Count > 24,
            Search = search, DeviceType = deviceType, Architecture = architecture, TeamOnly = teamOnly,
            Architectures = await catalog.PublishedArchitecturesAsync()
        });
    }

    [HttpGet("/hardware/{slug}")]
    public async Task<IActionResult> Details(string slug)
    {
        var device = await db.Set<Hardware>().AsNoTracking().Include(x => x.Translations)
            .SingleOrDefaultAsync(x => x.Slug == slug && x.Publication == HardwarePublication.Published);
        return device == null ? NotFound() : this.SimpleView(new HardwareDetailsModel
        {
            Device = device, Text = HardwareCatalog.TextFor(device)
        });
    }
}
