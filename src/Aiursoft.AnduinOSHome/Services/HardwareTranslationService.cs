using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Globalization;
using System.Reflection;
using Aiursoft.AnduinOSHome.Entities;
using Aiursoft.Scanner.Abstractions;
using Aiursoft.WebTools.OfficialPlugins;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.AnduinOSHome.Services;

public class HardwareTranslationService(
    AnduinOSHomeDbContext db,
    IHardwareTextTranslator translator,
    ILogger<HardwareTranslationService> logger) : IScopedDependency
{
    private static readonly PropertyInfo[] TextFields = typeof(HardwareTranslation).GetProperties()
        .Where(x => x.PropertyType == typeof(string) && x.Name != nameof(HardwareTranslation.Culture)).ToArray();

    public static IEnumerable<string> TargetCultures(string sourceCulture) =>
        LocalizationPlugin.SupportedCultures.Keys
            .Where(x => !SameLanguage(x, sourceCulture)).Distinct(StringComparer.OrdinalIgnoreCase);

    private static bool SameLanguage(string a, string b) =>
        a.Equals(b, StringComparison.OrdinalIgnoreCase) ||
        (a.StartsWith("en", StringComparison.OrdinalIgnoreCase) &&
         b.StartsWith("en", StringComparison.OrdinalIgnoreCase) &&
         CultureInfo.GetCultureInfo(a).TwoLetterISOLanguageName == "en" &&
         CultureInfo.GetCultureInfo(b).TwoLetterISOLanguageName == "en");

    public async Task TranslateAsync(int hardwareId)
    {
        var device = await db.Hardware.AsNoTracking().Include(x => x.Translations)
            .SingleOrDefaultAsync(x => x.Id == hardwareId);
        var source = device?.Translations.SingleOrDefault(x => x.Culture == device.SourceCulture);
        if (device == null || source == null || string.IsNullOrWhiteSpace(source.Description) ||
            device.Publication == HardwarePublication.Archived) return;

        var failures = new List<Exception>();
        foreach (var culture in TargetCultures(device.SourceCulture))
        {
            var previous = device.Translations.SingleOrDefault(x => x.Culture == culture);
            if (previous != null && previous.BasedOnSourceRevision == device.SourceRevision) continue;
            try
            {
                var translatedValues = new Dictionary<PropertyInfo, string?>(TextFields.Length);
                foreach (var field in TextFields)
                {
                    var original = (string?)field.GetValue(source);
                    var value = string.IsNullOrWhiteSpace(original) ? original :
                        await translator.TranslateAsync(original, culture);
                    var limit = field.GetCustomAttribute<MaxLengthAttribute>()!.Length;
                    if (!string.IsNullOrWhiteSpace(original) && (string.IsNullOrWhiteSpace(value) || value.Length > limit))
                        throw new InvalidOperationException($"Invalid translated field: {field.Name} ({culture}).");
                    translatedValues[field] = value;
                }

                // Do not hold a database transaction during the network calls. Check the source again
                // inside a short serializable transaction wrapped in the configured execution strategy
                // so retrying providers (such as Pomelo MySQL) can execute user-initiated transactions.
                var strategy = db.Database.CreateExecutionStrategy();
                var shouldContinue = await strategy.ExecuteAsync(async () =>
                {
                    db.ChangeTracker.Clear();
                    await using var transaction = db.Database.IsRelational()
                        ? await db.Database.BeginTransactionAsync(IsolationLevel.Serializable) : null;
                    var current = await db.Hardware.SingleOrDefaultAsync(x => x.Id == hardwareId);
                    if (current == null || current.SourceRevision != device.SourceRevision ||
                        current.Publication == HardwarePublication.Archived) return false;
                    var target = await db.HardwareTranslations
                        .SingleOrDefaultAsync(x => x.HardwareId == hardwareId && x.Culture == culture);
                    if (target == null)
                    {
                        var translated = new HardwareTranslation
                        {
                            HardwareId = hardwareId,
                            Culture = culture,
                            BasedOnSourceRevision = device.SourceRevision
                        };
                        foreach (var (field, value) in translatedValues) field.SetValue(translated, value);
                        db.HardwareTranslations.Add(translated);
                    }
                    else if (target.BasedOnSourceRevision == previous?.BasedOnSourceRevision &&
                             TextFields.All(x => Equals(x.GetValue(target), x.GetValue(previous))))
                    {
                        foreach (var (field, value) in translatedValues) field.SetValue(target, value);
                        target.BasedOnSourceRevision = device.SourceRevision;
                    }
                    else return true; // An editor saved this language while translation was running.
                    await db.SaveChangesAsync();
                    if (transaction != null) await transaction.CommitAsync();
                    return true;
                });
                if (!shouldContinue) return;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Could not translate hardware {HardwareId} to {Culture}", hardwareId, culture);
                failures.Add(ex);
            }
        }
        if (failures.Count > 0) throw new AggregateException("Some hardware translations failed. They will be retried by the scheduled job.", failures);
    }
}
