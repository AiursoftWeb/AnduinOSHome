using Aiursoft.AnduinOSHome.Entities;
using Aiursoft.AnduinOSHome.Services;
using Aiursoft.AnduinOSHome.Services.BackgroundJobs;
using Aiursoft.AnduinOSHome.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Caching.Memory;

namespace Aiursoft.AnduinOSHome.Tests.UnitTests;

[TestClass]
public class HardwareTranslationTests
{
    private SqliteContext db = null!;
    private Hardware device = null!;
    private FakeTranslator translator = null!;
    private HardwareTranslationService service = null!;

    [TestInitialize]
    public async Task Initialize()
    {
        db = new SqliteContext(new DbContextOptionsBuilder<SqliteContext>().UseSqlite("DataSource=:memory:").Options);
        await db.Database.OpenConnectionAsync();
        await db.Database.EnsureCreatedAsync();
        device = new Hardware { Slug = "test", Brand = "Example", Model = "Desktop", Architecture = "AMD64",
            SourceRevision = 1, Translations = [new HardwareTranslation { Culture = "en", BasedOnSourceRevision = 1,
                Description = "Description", FirmwareNotes = "Firmware", InstallationNotes = "Install", KnownIssues = "Issues",
                InstallationDetail = "Installation", PerformanceDetail = "Performance", SecureBootDetail = "SecureBoot",
                WifiDetail = "Wifi", GraphicsDetail = "Graphics", VirtualizationDetail = "Virtualization",
                DisplayDetail = "Display", ConfigurationText = "Configuration", ImageCreditText = "Credit" }] };
        db.Hardware.Add(device);
        await db.SaveChangesAsync();
        translator = new FakeTranslator();
        service = new HardwareTranslationService(db, translator, NullLogger<HardwareTranslationService>.Instance);
    }

    [TestCleanup]
    public async Task Cleanup() => await db.DisposeAsync();

    private async Task<Hardware> Read()
    {
        db.ChangeTracker.Clear();
        return await db.Hardware.Include(x => x.Translations).SingleAsync();
    }

    [TestMethod]
    public async Task AllFieldsAreTranslatedAndRerunningIsIdempotent()
    {
        await service.TranslateAsync(device.Id);
        var result = await Read();
        var targets = HardwareTranslationService.TargetCultures("en").ToArray();
        Assert.AreEqual(targets.Length * 13, translator.Calls);
        Assert.AreEqual(targets.Length + 1, result.Translations.Count);
        var fields = typeof(HardwareTranslation).GetProperties()
            .Where(x => x.PropertyType == typeof(string) && x.Name != "Culture").ToArray();
        var source = result.Translations.Single(x => x.Culture == "en");
        foreach (var translation in result.Translations.Where(x => x.Culture != "en"))
        {
            Assert.AreEqual(1, translation.BasedOnSourceRevision);
            foreach (var field in fields)
                Assert.AreEqual($"[{translation.Culture}] {field.GetValue(source)}", field.GetValue(translation));
        }
        var calls = translator.Calls;
        await service.TranslateAsync(device.Id);
        Assert.AreEqual(calls, translator.Calls);
    }

    [TestMethod]
    public async Task SourceChangesRefreshOutdatedTranslationsButPreserveCurrentManualText()
    {
        db.HardwareTranslations.Add(new HardwareTranslation { HardwareId = device.Id, Culture = "zh-CN",
            BasedOnSourceRevision = 0, Description = "Old" });
        db.HardwareTranslations.Add(new HardwareTranslation { HardwareId = device.Id, Culture = "de-DE",
            BasedOnSourceRevision = 1, Description = "Manual" });
        await db.SaveChangesAsync();
        await service.TranslateAsync(device.Id);
        var result = await Read();
        Assert.AreEqual("[zh-CN] Description", result.Translations.Single(x => x.Culture == "zh-CN").Description);
        Assert.AreEqual("Manual", result.Translations.Single(x => x.Culture == "de-DE").Description);
        result.SourceRevision++;
        result.Translations.Single(x => x.Culture == "en").Description = "Updated";
        await db.SaveChangesAsync();
        Assert.AreEqual("Updated", HardwareCatalog.TextFor(result, "zh-CN").Description);
        await service.TranslateAsync(device.Id);
        Assert.AreEqual("[zh-CN] Updated", HardwareCatalog.TextFor(await Read(), "zh-CN").Description);
    }

    [TestMethod]
    public async Task FailedFieldDoesNotSavePartialLanguageAndCanBeRetried()
    {
        translator.Callback = (_, language) => language == "zh-CN" ? throw new InvalidOperationException("Offline") : Task.CompletedTask;
        await Assert.ThrowsExactlyAsync<AggregateException>(() => service.TranslateAsync(device.Id));
        Assert.IsFalse((await Read()).Translations.Any(x => x.Culture == "zh-CN"));
        translator.Callback = null;
        await service.TranslateAsync(device.Id);
        Assert.IsTrue((await Read()).Translations.Any(x => x.Culture == "zh-CN"));
    }

    [TestMethod]
    public async Task SourceEditDuringTranslationDiscardsSnapshot()
    {
        translator.Callback = async (_, _) =>
        {
            translator.Callback = null;
            await db.Hardware.Where(x => x.Id == device.Id).ExecuteUpdateAsync(x => x.SetProperty(h => h.SourceRevision, 2));
        };
        await service.TranslateAsync(device.Id);
        Assert.AreEqual(1, (await Read()).Translations.Count);
    }

    [TestMethod]
    public async Task ManualEditDuringTranslationIsNotOverwritten()
    {
        var first = HardwareTranslationService.TargetCultures("en").First();
        db.HardwareTranslations.Add(new HardwareTranslation { HardwareId = device.Id, Culture = first,
            BasedOnSourceRevision = 0, Description = "Old" });
        await db.SaveChangesAsync();
        translator.Callback = async (_, _) =>
        {
            translator.Callback = null;
            await db.HardwareTranslations.Where(x => x.Culture == first).ExecuteUpdateAsync(x =>
                x.SetProperty(t => t.Description, "Manual").SetProperty(t => t.BasedOnSourceRevision, 1));
        };
        await service.TranslateAsync(device.Id);
        Assert.AreEqual("Manual", (await Read()).Translations.Single(x => x.Culture == first).Description);
    }

    [TestMethod]
    public async Task EmptyOptionalFieldsDoNotCallTranslator()
    {
        var source = device.Translations.Single();
        foreach (var field in typeof(HardwareTranslation).GetProperties().Where(x => x.PropertyType == typeof(string) &&
                     x.Name != "Culture" && x.Name != "Description")) field.SetValue(source, null);
        source.FirmwareNotes = " ";
        await db.SaveChangesAsync();
        await service.TranslateAsync(device.Id);
        Assert.AreEqual(HardwareTranslationService.TargetCultures("en").Count(), translator.Calls);
        Assert.IsNull((await Read()).Translations.First(x => x.Culture != "en").KnownIssues);
    }

    [TestMethod]
    public async Task DisabledJobDoesNotTranslate()
    {
        var settings = new GlobalSettingsService(db,
            new ConfigurationBuilder().Build(), null!,
            new MemoryCache(new MemoryCacheOptions()), new Microsoft.AspNetCore.DataProtection.EphemeralDataProtectionProvider(), NullLogger<GlobalSettingsService>.Instance);
        var job = new HardwareTranslationJob(db, service, settings);
        await job.ExecuteAsync();
        Assert.AreEqual(0, translator.Calls);
    }

    private sealed class FakeTranslator : IHardwareTextTranslator
    {
        public int Calls { get; private set; }
        public Func<string, string, Task>? Callback { get; set; }
        public async Task<string> TranslateAsync(string text, string language)
        {
            Calls++;
            if (Callback != null) await Callback(text, language);
            return $"[{language}] {text}";
        }
    }

}
