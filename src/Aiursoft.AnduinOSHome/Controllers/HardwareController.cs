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
    public async Task<IActionResult> Index(int page = 1)
    {
        if (page < 1 || page > 100000) return BadRequest();
        var items = await catalog.PublishedAsync(page: page);
        return this.SimpleView(new HardwareIndexModel
        {
            Items = items.Take(24).ToList(), Page = page, HasNextPage = items.Count > 24
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
