using System.Globalization;
using Aiursoft.AnduinOSHome.Entities;
using Aiursoft.Scanner.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.AnduinOSHome.Services;

public class HardwareCatalog(AnduinOSHomeDbContext db) : IScopedDependency
{
    public Task<List<Hardware>> PublishedAsync(bool featured = false, int page = 1,
        string? search = null, HardwareDeviceType? deviceType = null, string? architecture = null,
        bool teamOnly = false)
    {
        var query = db.Set<Hardware>().AsNoTracking().Include(x => x.Translations)
            .Where(x => x.Publication == HardwarePublication.Published && (!featured || x.Featured));
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLowerInvariant();
            query = query.Where(x => (x.Brand + " " + x.Model).ToLower().Contains(term)
                || (x.Sku != null && x.Sku.ToLower().Contains(term))
                || (x.Configuration != null && x.Configuration.ToLower().Contains(term)));
        }
        if (deviceType.HasValue) query = query.Where(x => x.DeviceType == deviceType.Value);
        if (!string.IsNullOrWhiteSpace(architecture)) query = query.Where(x => x.Architecture == architecture);
        if (teamOnly) query = query.Where(x => x.TeamDevice);
        return query.OrderBy(x => x.SortOrder).ThenBy(x => x.Id)
            .Skip(featured ? 0 : (page - 1) * 24).Take(featured ? 6 : 25).ToListAsync();
    }

    public Task<List<string>> PublishedArchitecturesAsync() =>
        db.Set<Hardware>().AsNoTracking().Where(x => x.Publication == HardwarePublication.Published)
            .Select(x => x.Architecture).Distinct().OrderBy(x => x).ToListAsync();

    public static HardwareTranslation TextFor(Hardware device, string? culture = null)
    {
        var name = culture ?? CultureInfo.CurrentUICulture.Name;
        var parent = name.Split('-')[0];
        return device.Translations.FirstOrDefault(x => x.Culture.Equals(name, StringComparison.OrdinalIgnoreCase))
            ?? device.Translations.FirstOrDefault(x => x.Culture.Equals(parent, StringComparison.OrdinalIgnoreCase))
            ?? device.Translations.FirstOrDefault(x => x.Culture.StartsWith(parent + "-", StringComparison.OrdinalIgnoreCase))
            ?? device.Translations.FirstOrDefault(x => x.Culture.Equals(device.SourceCulture, StringComparison.OrdinalIgnoreCase))
            ?? new HardwareTranslation();
    }
}
