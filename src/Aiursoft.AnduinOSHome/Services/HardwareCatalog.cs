using System.Globalization;
using Aiursoft.AnduinOSHome.Entities;
using Aiursoft.Scanner.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.AnduinOSHome.Services;

public class HardwareCatalog(AnduinOSHomeDbContext db) : IScopedDependency
{
    public Task<List<Hardware>> PublishedAsync(bool featured = false, int page = 1) =>
        db.Set<Hardware>().AsNoTracking().Include(x => x.Translations)
            .Where(x => x.Publication == HardwarePublication.Published && (!featured || x.Featured))
            .OrderBy(x => x.SortOrder).ThenBy(x => x.Id)
            .Skip(featured ? 0 : (page - 1) * 24).Take(featured ? 6 : 25).ToListAsync();

    public static HardwareTranslation TextFor(Hardware device, string? culture = null)
    {
        var name = culture ?? CultureInfo.CurrentUICulture.Name;
        var parent = name.Split('-')[0];
        return device.Translations.FirstOrDefault(x => x.Culture.Equals(name, StringComparison.OrdinalIgnoreCase))
            ?? device.Translations.FirstOrDefault(x => x.Culture.Equals(parent, StringComparison.OrdinalIgnoreCase))
            ?? device.Translations.FirstOrDefault(x => x.Culture == "en")
            ?? new HardwareTranslation();
    }
}
