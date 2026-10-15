using Aiursoft.AnduinOSHome.Entities;
using Aiursoft.Canon.BackgroundJobs;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.AnduinOSHome.Services.BackgroundJobs;

public class HardwareTranslationJob(
    AnduinOSHomeDbContext db,
    HardwareTranslationService translations,
    GlobalSettingsService settingsService) : IBackgroundJob
{
    public string Name => "Translate hardware content";
    public string Description => "Generate missing hardware translations and refresh translations after source changes.";

    public async Task ExecuteAsync()
    {
        if (!await settingsService.IsAiLocalizationEnabledAsync()) return;
        var failures = new List<Exception>();
        var lastId = 0;
        while (await settingsService.IsAiLocalizationEnabledAsync())
        {
            var pageStartId = lastId;
            var devices = await db.Hardware.AsNoTracking().Include(x => x.Translations)
                .Where(x => x.Id > pageStartId && x.Publication != HardwarePublication.Archived)
                .OrderBy(x => x.Id).Take(20).ToListAsync();
            if (devices.Count == 0) break;
            lastId = devices[^1].Id;
            foreach (var device in devices.Where(x => HardwareTranslationService.TargetCultures(x.SourceCulture)
                         .Any(culture => !x.Translations.Any(t => t.Culture == culture &&
                             t.BasedOnSourceRevision == x.SourceRevision))))
            {
                try { await translations.TranslateAsync(device.Id); }
                catch (Exception ex) { failures.Add(ex); }
            }
        }
        if (failures.Count > 0) throw new AggregateException(failures);
    }
}
