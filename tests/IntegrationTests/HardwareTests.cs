using System.Net;
using Aiursoft.AnduinOSHome.Entities;
using Aiursoft.AnduinOSHome.Services;
using Aiursoft.AnduinOSHome.Services.FileStorage;
using Aiursoft.AnduinOSHome.Sqlite;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;

namespace Aiursoft.AnduinOSHome.Tests.IntegrationTests;

[TestClass]
public class HardwareTests : TestBase
{
    private static readonly HttpClient Anonymous = new();

    [TestMethod]
    public void CompatibilityVerdictsDistinguishVerifiedPartialBrokenAndUnknown()
    {
        Assert.AreEqual("positive", HardwareLabels.StatusTone(HardwarePerformance.Ideal));
        Assert.AreEqual("positive", HardwareLabels.StatusTone(HardwareGraphics.AutomaticInstallation));
        Assert.AreEqual("caution", HardwareLabels.StatusTone(HardwarePerformance.Adequate));
        Assert.AreEqual("caution", HardwareLabels.StatusTone(HardwareGraphics.ManualSetup));
        Assert.AreEqual("negative", HardwareLabels.StatusTone(HardwareSupport.Unsupported));
        Assert.AreEqual("unknown", HardwareLabels.StatusTone(HardwareSupport.Untested));
        Assert.AreEqual("unknown", HardwareLabels.StatusTone(HardwareSupport.NotApplicable));
        Assert.AreEqual("check", HardwareLabels.StatusIcon(HardwareEase.Straightforward));
        Assert.AreEqual("alert-triangle", HardwareLabels.StatusIcon(HardwareEase.Difficult));
        Assert.AreEqual("alert-triangle", HardwareLabels.StatusIcon(HardwareEase.Unsupported));
        Assert.AreEqual("minus", HardwareLabels.StatusIcon(HardwareEase.Untested));
    }

    [TestMethod]
    public async Task DetailRendersEveryVerdictTone()
    {
        var item = await Read(await AddDevice(HardwarePublication.Published));
        using (var scope = Server!.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AnduinOSHomeDbContext>();
            var device = await db.Hardware.SingleAsync(x => x.Id == item.Id);
            device.Installation = HardwareEase.Difficult;
            device.Performance = HardwarePerformance.Insufficient;
            device.SecureBoot = HardwareSupport.Supported;
            await db.SaveChangesAsync();
        }

        var html = await Http.GetStringAsync("/hardware/" + item.Slug);
        Assert.Contains("hardware-verdict--positive", html);
        Assert.Contains("hardware-verdict--caution", html);
        Assert.Contains("hardware-verdict--negative", html);
        Assert.Contains("hardware-verdict--unknown", html);
    }

    private async Task<int> AddDevice(HardwarePublication publication, bool featured = true)
    {
        using var scope = Server!.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AnduinOSHomeDbContext>();
        var item = new Hardware
        {
            Brand = "Example", Model = "Test workstation", Slug = Guid.NewGuid().ToString("N"),
            Architecture = "ARM64", Publication = publication, Featured = featured,
            Translations = [new HardwareTranslation { Culture = "en", Description = "English device description" },
                new HardwareTranslation { Culture = "de", Description = "Deutsche Beschreibung" }]
        };
        db.Hardware.Add(item);
        await db.SaveChangesAsync();
        return item.Id;
    }

    private async Task<Hardware> Read(int id)
    {
        using var scope = Server!.Services.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<AnduinOSHomeDbContext>().Hardware
            .Include(x => x.Translations).SingleAsync(x => x.Id == id);
    }

    private static Dictionary<string, string> Form(int id, string slug, string culture = "en") => new()
    {
        ["Device.Id"] = id.ToString(), ["Device.Brand"] = "Example", ["Device.Model"] = "Test workstation",
        ["Device.Slug"] = slug, ["Device.Architecture"] = "AMD64", ["Text.Culture"] = culture,
        ["Text.Description"] = "Saved description", ["Device.Publication"] = "1", ["Device.Featured"] = "true"
    };

    [TestMethod]
    public async Task PublishedOnlyAndPlacement()
    {
        var published = await Read(await AddDevice(HardwarePublication.Published));
        var draft = await Read(await AddDevice(HardwarePublication.Draft));
        var archived = await Read(await AddDevice(HardwarePublication.Archived));
        var html = await Http.GetStringAsync("/");
        Assert.Contains("recommended-hardware", html);
        Assert.Contains(published.Slug, html);
        Assert.Contains("Price not listed", html);
        Assert.DoesNotContain("English device description", html);
        Assert.DoesNotContain(draft.Slug, html);
        Assert.IsTrue(html.IndexOf("Windows Central", StringComparison.Ordinal) < html.IndexOf("id=\"recommended-hardware\"", StringComparison.Ordinal));
        Assert.IsTrue(html.IndexOf("id=\"recommended-hardware\"", StringComparison.Ordinal) < html.IndexOf("accordionFaq", StringComparison.Ordinal));
        var details = await Http.GetAsync("/hardware/" + published.Slug);
        Assert.AreEqual(HttpStatusCode.OK, details.StatusCode);
        var detailsHtml = await details.Content.ReadAsStringAsync();
        Assert.Contains("English device description", detailsHtml);
        Assert.Contains("class=\"hardware-detail\"", detailsHtml);
        Assert.Contains("/css/hardware.css", detailsHtml);
        Assert.AreEqual(HttpStatusCode.NotFound, (await Http.GetAsync("/hardware/" + draft.Slug)).StatusCode);
        Assert.AreEqual(HttpStatusCode.NotFound, (await Http.GetAsync("/hardware/" + archived.Slug)).StatusCode);
    }

    [TestMethod]
    public async Task AdminCanCreateTranslateAndArchive()
    {
        await LoginAsAdmin();
        var slug = Guid.NewGuid().ToString("N");
        var response = await PostForm("/ManageHardware/Edit/0", Form(0, slug), "/ManageHardware/Edit");
        Assert.AreEqual(HttpStatusCode.Found, response.StatusCode, await response.Content.ReadAsStringAsync());
        using var scope = Server!.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AnduinOSHomeDbContext>();
        var id = await db.Hardware.Where(x => x.Slug == slug).Select(x => x.Id).SingleAsync();
        var translated = Form(id, slug, "zh-CN");
        translated["Text.Description"] = "中文介绍";
        response = await PostForm("/ManageHardware/Edit/" + id, translated, "/ManageHardware/Edit/" + id);
        Assert.AreEqual(HttpStatusCode.Found, response.StatusCode);
        var item = await Read(id);
        Assert.AreEqual(2, item.Translations.Count);
        Assert.AreEqual("中文介绍", HardwareCatalog.TextFor(item, "zh-CN").Description);
        Assert.AreEqual("Saved description", HardwareCatalog.TextFor(item, "fr-FR").Description);
        translated["Device.Publication"] = "2";
        await PostForm("/ManageHardware/Edit/" + id, translated, "/ManageHardware/Edit/" + id);
        Assert.AreEqual(HttpStatusCode.NotFound, (await Http.GetAsync("/hardware/" + slug)).StatusCode);
    }

    [TestMethod]
    public async Task AnonymousAndOrdinaryUsersCannotManageHardware()
    {
        Assert.AreEqual(HttpStatusCode.Found, (await Http.GetAsync("/ManageHardware/Edit")).StatusCode);
        await RegisterAndLoginAsync();
        var response = await Http.GetAsync("/ManageHardware/Edit");
        Assert.IsTrue(response.StatusCode is HttpStatusCode.Found or HttpStatusCode.Forbidden);
    }

    [TestMethod]
    public async Task RejectsUnsafePathsUrlsEnumsAndMissingEnglish()
    {
        await LoginAsAdmin();
        foreach (var (key, value) in new[]
        {
            ("Device.ProductImagePath", "avatar/test.png"),
            ("Device.ProductImagePath", "hardware/../../secret.png"),
            ("Device.ProductImagePath", "hardware/missing.png"),
            ("Device.ProductUrl", "javascript:alert(1)"),
            ("Device.Publication", "99"),
            ("Device.Installation", "99"),
            ("Device.PriceUsd", "-1"),
            ("Text.Culture", " ")
        })
        {
            var slug = Guid.NewGuid().ToString("N");
            var form = Form(0, slug);
            form[key] = value;
            var response = await PostForm("/ManageHardware/Edit/0", form, "/ManageHardware/Edit");
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(HttpStatusCode.NotFound, (await Http.GetAsync("/hardware/" + slug)).StatusCode);
        }
        var missingEnglish = await PostForm("/ManageHardware/Edit/0", Form(0, "non-english", "de"), "/ManageHardware/Edit");
        Assert.AreEqual(HttpStatusCode.OK, missingEnglish.StatusCode);
        Assert.Contains("Save an English description", await missingEnglish.Content.ReadAsStringAsync());
    }

    [TestMethod]
    public async Task RejectsMissingCsrfAndEscapesContent()
    {
        await LoginAsAdmin();
        Assert.AreEqual(HttpStatusCode.BadRequest, (await PostForm("/ManageHardware/Edit/0",
            Form(0, "no-csrf"), includeToken: false)).StatusCode);
        var form = Form(0, "escaped-content");
        form["Text.Description"] = "<script>alert(1)</script>";
        await PostForm("/ManageHardware/Edit/0", form, "/ManageHardware/Edit");
        var html = await Http.GetStringAsync("/hardware/escaped-content");
        Assert.Contains("&lt;script&gt;", html);
        Assert.DoesNotContain("<script>alert(1)</script>", html);
    }

    [TestMethod]
    public async Task PublicImageUploadPreservesTransparencyAndCanBeDetached()
    {
        await LoginAsAdmin();
        using var scope = Server!.Services.CreateScope();
        var storage = scope.ServiceProvider.GetRequiredService<StorageService>();
        using var bitmap = new SKBitmap(8, 8);
        bitmap.Erase(SKColors.Transparent);
        bitmap.SetPixel(0, 0, SKColors.Blue);
        using var image = SKImage.FromBitmap(bitmap);
        using var png = image.Encode(SKEncodedImageFormat.Png, 100);
        using var body = new MultipartFormDataContent();
        body.Add(new ByteArrayContent(png.ToArray()), "file", "product.png");
        var upload = await Http.PostAsync(storage.GetUploadUrl("hardware", maxSizeInMb: 10,
            allowedExtensions: "png jpg jpeg webp"), body);
        upload.EnsureSuccessStatusCode();
        var json = await upload.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        var path = json.GetProperty("Path").GetString()!;
        Assert.StartsWith("hardware/", path);
        var form = Form(0, "image-device");
        form["Device.ProductImagePath"] = path;
        form["Device.ExperienceImagePath"] = path;
        var saved = await PostForm("/ManageHardware/Edit/0", form, "/ManageHardware/Edit");
        Assert.AreEqual(HttpStatusCode.Found, saved.StatusCode);
        var download = await Anonymous.GetAsync(new Uri(Http.BaseAddress!, "/download/" + path + "?w=8"));
        download.EnsureSuccessStatusCode();
        Assert.AreEqual("inline", download.Content.Headers.ContentDisposition?.DispositionType);
        Assert.AreEqual("nosniff", download.Headers.GetValues("X-Content-Type-Options").Single());
        using var decoded = SKBitmap.Decode(await download.Content.ReadAsByteArrayAsync());
        Assert.AreEqual((byte)0, decoded.GetPixel(7, 7).Alpha);
        var db = scope.ServiceProvider.GetRequiredService<AnduinOSHomeDbContext>();
        var id = await db.Hardware.Where(x => x.Slug == "image-device").Select(x => x.Id).SingleAsync();
        form["Device.Id"] = id.ToString();
        form["ClearProductImage"] = "true";
        form["ClearExperienceImage"] = "true";
        saved = await PostForm("/ManageHardware/Edit/" + id, form, "/ManageHardware/Edit/" + id);
        Assert.AreEqual(HttpStatusCode.Found, saved.StatusCode);
        Assert.IsNull((await Read(id)).ProductImagePath);
        Assert.IsNull((await Read(id)).ExperienceImagePath);
    }

    [TestMethod]
    public async Task RejectsFakeImageAndDuplicateAddress()
    {
        await LoginAsAdmin();
        using var scope = Server!.Services.CreateScope();
        var storage = scope.ServiceProvider.GetRequiredService<StorageService>();
        using var fake = new MemoryStream("not an image"u8.ToArray());
        var path = "hardware/" + Guid.NewGuid().ToString("N") + ".png";
        await storage.SaveFromStream(path, fake);
        var form = Form(0, "invalid-image");
        form["Device.ProductImagePath"] = path;
        Assert.AreEqual(HttpStatusCode.OK, (await PostForm("/ManageHardware/Edit/0", form, "/ManageHardware/Edit")).StatusCode);
        var device = await Read(await AddDevice(HardwarePublication.Published));
        var response = await PostForm("/ManageHardware/Edit/0", Form(0, device.Slug), "/ManageHardware/Edit");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("already in use", await response.Content.ReadAsStringAsync());
    }

    [TestMethod]
    public async Task SqliteMigrationSupportsHardwareAndUniqueTranslations()
    {
        var options = new DbContextOptionsBuilder<SqliteContext>().UseSqlite("DataSource=:memory:").Options;
        await using var db = new SqliteContext(options);
        await db.Database.OpenConnectionAsync();
        await db.Database.MigrateAsync();
        var item = new Hardware { Slug = "example", Brand = "Example", Model = "Desktop", Architecture = "AMD64",
            Translations = [new HardwareTranslation { Culture = "en", Description = "Test" }] };
        db.Hardware.Add(item);
        await db.SaveChangesAsync();
        db.HardwareTranslations.Add(new HardwareTranslation { HardwareId = item.Id, Culture = "en", Description = "Duplicate" });
        await Assert.ThrowsExactlyAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    [TestMethod]
    public void TranslationFallsBackToParentThenEnglish()
    {
        var device = new Hardware { Translations = [
            new HardwareTranslation { Culture = "en", Description = "English" },
            new HardwareTranslation { Culture = "de", Description = "Deutsch" }] };
        Assert.AreEqual("Deutsch", HardwareCatalog.TextFor(device, "de-DE").Description);
        Assert.AreEqual("English", HardwareCatalog.TextFor(device, "ja-JP").Description);
    }
}
