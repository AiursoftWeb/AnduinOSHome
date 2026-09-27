using System.Net;
using Aiursoft.AnduinOSHome.Entities;
using Aiursoft.AnduinOSHome.Services;
using Aiursoft.AnduinOSHome.Services.FileStorage;
using Aiursoft.AnduinOSHome.Models.HardwareViewModels;
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

    [TestMethod]
    public async Task CapabilityDetailsAreOptionalEncodedAndFallBackToEnglish()
    {
        var item = await Read(await AddDevice(HardwarePublication.Published));
        using (var scope = Server!.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AnduinOSHomeDbContext>();
            var english = await db.HardwareTranslations.SingleAsync(x => x.HardwareId == item.Id && x.Culture == "en");
            english.VirtualizationDetail = "KVM <verified>";
            english.DisplayDetail = "External monitor scaling depends on the monitor.";
            await db.SaveChangesAsync();
        }

        var html = await Http.GetStringAsync("/hardware/" + item.Slug);
        Assert.IsTrue(System.Text.RegularExpressions.Regex.IsMatch(html, "data-hardware-detail[^>]*>KVM &lt;verified&gt;</span>"));
        Assert.IsTrue(System.Text.RegularExpressions.Regex.IsMatch(html, "data-hardware-detail[^>]*>External monitor scaling depends on the monitor.</span>"));
        Assert.DoesNotContain("KVM <verified>", html);
        Assert.Contains("data-hardware-dialog", html);
        Assert.Contains("/js/hardware-details.js", html);
        Assert.AreEqual(7, System.Text.RegularExpressions.Regex.Matches(html, "data-hardware-insight(?:\\s|>)").Count);
        Assert.Contains("disabled=\"disabled\"", html);

        var translated = await Read(item.Id);
        var model = new HardwareDetailsModel { Device = translated, Text = HardwareCatalog.TextFor(translated, "de") };
        Assert.AreEqual("KVM <verified>", model.Detail(x => x.VirtualizationDetail));
        Assert.IsNull(model.Detail(x => x.WifiDetail));
        Assert.IsNull(model.Configuration);
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
    public async Task CatalogSearchAndFiltersUsePublishedDeviceMetadata()
    {
        var matching = await Read(await AddDevice(HardwarePublication.Published));
        var other = await Read(await AddDevice(HardwarePublication.Published));
        var searchTerm = "GX10-" + Guid.NewGuid().ToString("N");
        using (var scope = Server!.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AnduinOSHomeDbContext>();
            var device = await db.Hardware.SingleAsync(x => x.Id == matching.Id);
            device.Sku = searchTerm;
            device.DeviceType = HardwareDeviceType.HomeSupercomputer;
            device.TeamDevice = true;
            device.Architecture = "ARM64";
            await db.SaveChangesAsync();
        }

        var url = "/Hardware?search=" + searchTerm + "&deviceType=HomeSupercomputer&architecture=ARM64&teamOnly=true";
        var html = await Http.GetStringAsync(url);
        Assert.Contains(matching.Slug, html);
        Assert.Contains(searchTerm, html);
        Assert.Contains("hardware-catalog", html);
        Assert.Contains("Home supercomputer", html);
        Assert.DoesNotContain(other.Slug, html);
        Assert.Contains("name=\"search\"", html);
        Assert.Contains("name=\"deviceType\"", html);
        Assert.Contains("name=\"architecture\"", html);
        Assert.Contains("name=\"teamOnly\"", html);

        html = await Http.GetStringAsync("/Hardware?search=" + searchTerm + "&deviceType=Laptop");
        Assert.DoesNotContain(matching.Slug, html);
        Assert.Contains("No matching hardware yet", html);
        Assert.AreEqual(HttpStatusCode.BadRequest, (await Http.GetAsync("/Hardware?deviceType=999")).StatusCode);
        Assert.AreEqual(HttpStatusCode.BadRequest, (await Http.GetAsync("/Hardware?search=" + new string('x', 101))).StatusCode);
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
        response = await PostForm($"/ManageHardware/SaveTranslation/{id}", new Dictionary<string, string>
        {
            ["Culture"] = "zh-CN", ["Description"] = "中文介绍",
            ["VirtualizationDetail"] = "KVM 已验证",
            ["ConfigurationText"] = "20 核 Arm CPU，128 GB 统一内存",
            ["ImageCreditText"] = "图片由厂商提供"
        }, $"/ManageHardware/Localize/{id}");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var item = await Read(id);
        Assert.AreEqual(2, item.Translations.Count);
        Assert.AreEqual("中文介绍", HardwareCatalog.TextFor(item, "zh-CN").Description);
        Assert.AreEqual("KVM 已验证", HardwareCatalog.TextFor(item, "zh-CN").VirtualizationDetail);
        var localizedModel = new HardwareDetailsModel { Device = item, Text = HardwareCatalog.TextFor(item, "zh-CN") };
        Assert.AreEqual("20 核 Arm CPU，128 GB 统一内存", localizedModel.Configuration);
        Assert.AreEqual("图片由厂商提供", localizedModel.ImageCredit);
        Assert.AreEqual("Saved description", HardwareCatalog.TextFor(item, "fr-FR").Description);
        var archive = Form(id, slug);
        archive["Device.Publication"] = "2";
        await PostForm("/ManageHardware/Edit/" + id, archive, "/ManageHardware/Edit/" + id);
        Assert.AreEqual(HttpStatusCode.NotFound, (await Http.GetAsync("/hardware/" + slug)).StatusCode);
    }

    [TestMethod]
    public async Task TranslationEditorLoadsAndSavesOneLanguageWithoutChangingDeviceFacts()
    {
        await LoginAsAdmin();
        var id = await AddDevice(HardwarePublication.Published);
        var before = await Read(id);
        var page = await Http.GetStringAsync($"/ManageHardware/Localize/{id}");
        Assert.DoesNotContain("data-culture=\"en\"", page);
        Assert.Contains("data-culture=\"zh-CN\"", page);
        Assert.Contains("/js/hardware-localize.js", page);
        Assert.Contains("hardware-translation-form", page);

        Assert.AreEqual(HttpStatusCode.BadRequest,
            (await Http.GetAsync($"/ManageHardware/TranslationData/{id}?culture=en")).StatusCode);
        var emptyChinese = await Http.GetFromJsonAsync<System.Text.Json.JsonElement>(
            $"/ManageHardware/TranslationData/{id}?culture=zh-CN");
        Assert.AreEqual(string.Empty, emptyChinese.GetProperty("description").GetString());
        Assert.AreEqual("English device description", emptyChinese.GetProperty("source").GetProperty("description").GetString());

        var response = await PostForm($"/ManageHardware/SaveTranslation/{id}", new Dictionary<string, string>
        {
            ["Culture"] = "zh-CN", ["Description"] = "中文介绍",
            ["VirtualizationDetail"] = "KVM 已验证",
            ["ConfigurationText"] = "20 核 Arm CPU"
        }, $"/ManageHardware/Localize/{id}");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());
        var after = await Read(id);
        Assert.AreEqual(before.Slug, after.Slug);
        Assert.AreEqual(before.Architecture, after.Architecture);
        Assert.AreEqual(3, after.Translations.Count);
        Assert.AreEqual("中文介绍", HardwareCatalog.TextFor(after, "zh-CN").Description);
        Assert.AreEqual("KVM 已验证", HardwareCatalog.TextFor(after, "zh-CN").VirtualizationDetail);
        Assert.AreEqual("English device description", HardwareCatalog.TextFor(after, "en").Description);
        Assert.Contains("中文介绍", WebUtility.HtmlDecode(await Http.GetStringAsync(
            $"/ManageHardware/Preview/{id}?culture=zh-CN")));

        response = await PostForm($"/ManageHardware/SaveTranslation/{id}", new Dictionary<string, string>
        {
            ["Culture"] = "zh-CN", ["Description"] = "修改后的中文介绍"
        }, $"/ManageHardware/Localize/{id}");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(3, (await Read(id)).Translations.Count);
        Assert.AreEqual("修改后的中文介绍", HardwareCatalog.TextFor(await Read(id), "zh-CN").Description);
    }

    [TestMethod]
    public async Task TranslationEditorRequiresAdminCsrfAndValidText()
    {
        var id = await AddDevice(HardwarePublication.Published);
        Assert.AreEqual(HttpStatusCode.Found,
            (await Http.GetAsync($"/ManageHardware/Localize/{id}")).StatusCode);
        await RegisterAndLoginAsync();
        Assert.IsTrue((await Http.GetAsync($"/ManageHardware/Localize/{id}")).StatusCode
            is HttpStatusCode.Found or HttpStatusCode.Forbidden);

        await LoginAsAdmin();
        var url = $"/ManageHardware/SaveTranslation/{id}";
        var data = new Dictionary<string, string> { ["Culture"] = "fr-FR", ["Description"] = "Texte" };
        Assert.AreEqual(HttpStatusCode.BadRequest,
            (await PostForm(url, data, includeToken: false)).StatusCode);
        foreach (var invalid in new[]
        {
            new Dictionary<string, string> { ["Culture"] = " ", ["Description"] = "Texte" },
            new Dictionary<string, string> { ["Culture"] = "fr-FR", ["Description"] = " " },
            new Dictionary<string, string> { ["Culture"] = "fr-FR", ["Description"] = new string('x', 3001) }
        })
        {
            Assert.AreEqual(HttpStatusCode.BadRequest,
                (await PostForm(url, invalid, $"/ManageHardware/Localize/{id}")).StatusCode);
        }
        Assert.AreEqual(2, (await Read(id)).Translations.Count);
    }

    [TestMethod]
    public async Task SourceWorkflowPublishesWithoutEnglishAndTracksOnlyCopyChanges()
    {
        await LoginAsAdmin();
        var slug = Guid.NewGuid().ToString("N");
        var source = Form(0, slug, "zh-CN");
        source["Device.SourceCulture"] = "zh-CN";
        source["Text.Description"] = "原文评测";
        var saved = await PostForm("/ManageHardware/Edit/0", source, "/ManageHardware/Edit");
        Assert.AreEqual(HttpStatusCode.Found, saved.StatusCode);
        var device = await Read((await DbId(slug)));
        Assert.AreEqual("zh-CN", device.SourceCulture);
        Assert.AreEqual("原文评测", HardwareCatalog.TextFor(device, "fr-FR").Description);
        Assert.Contains("原文评测", WebUtility.HtmlDecode(await Http.GetStringAsync("/hardware/" + slug)));

        var draft = await Read(await AddDevice(HardwarePublication.Draft));
        Assert.AreEqual(HttpStatusCode.NotFound, (await Http.GetAsync("/hardware/" + draft.Slug)).StatusCode);
        Assert.AreEqual(HttpStatusCode.OK, (await Http.GetAsync($"/ManageHardware/Preview/{draft.Id}")).StatusCode);

        var id = device.Id;
        var translated = await PostForm($"/ManageHardware/SaveTranslation/{id}", new Dictionary<string, string>
        {
            ["Culture"] = "en", ["Description"] = "Translated review"
        }, $"/ManageHardware/Localize/{id}");
        Assert.AreEqual(HttpStatusCode.OK, translated.StatusCode);
        var revision = (await Read(id)).SourceRevision;
        source["Device.Id"] = id.ToString();
        source["Device.PriceUsd"] = "2999";
        saved = await PostForm($"/ManageHardware/Edit/{id}", source, $"/ManageHardware/Edit/{id}");
        Assert.AreEqual(HttpStatusCode.Found, saved.StatusCode);
        Assert.AreEqual(revision, (await Read(id)).SourceRevision);

        source["Text.Description"] = "更新后的原文";
        saved = await PostForm($"/ManageHardware/Edit/{id}", source, $"/ManageHardware/Edit/{id}");
        Assert.AreEqual(HttpStatusCode.Found, saved.StatusCode);
        device = await Read(id);
        Assert.AreEqual(revision + 1, device.SourceRevision);
        Assert.IsTrue(device.Translations.Single(x => x.Culture == "en").BasedOnSourceRevision < device.SourceRevision);
        Assert.Contains("Source updated", await Http.GetStringAsync($"/ManageHardware/Localize/{id}"));
    }

    [TestMethod]
    public async Task IncompleteSourceCanSaveDraftButCannotPublish()
    {
        await LoginAsAdmin();
        var slug = Guid.NewGuid().ToString("N");
        var form = Form(0, slug, "zh-CN");
        form["Device.SourceCulture"] = "zh-CN";
        form["Device.Publication"] = "0";
        form["Text.Description"] = "";
        var response = await PostForm("/ManageHardware/Edit/0", form, "/ManageHardware/Edit");
        Assert.AreEqual(HttpStatusCode.Found, response.StatusCode,
            System.Text.RegularExpressions.Regex.Match(await response.Content.ReadAsStringAsync(),
                "<div class=\"validation-summary-errors[^>]*>(.*?)</div>",
                System.Text.RegularExpressions.RegexOptions.Singleline).Value);
        var id = await DbId(slug);
        Assert.AreEqual(HardwarePublication.Draft, (await Read(id)).Publication);
        Assert.AreEqual(HttpStatusCode.OK, (await Http.GetAsync($"/ManageHardware/Preview/{id}")).StatusCode);
        form["Device.Id"] = id.ToString();
        form["Device.Publication"] = "1";
        response = await PostForm($"/ManageHardware/Edit/{id}", form, $"/ManageHardware/Edit/{id}");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(HardwarePublication.Draft, (await Read(id)).Publication);
    }

    [TestMethod]
    public async Task WorkspaceIndexSearchAndPublicationFilterShowEditorialState()
    {
        await LoginAsAdmin();
        var draft = await Read(await AddDevice(HardwarePublication.Draft));
        var published = await Read(await AddDevice(HardwarePublication.Published));
        var needle = "Search-" + Guid.NewGuid().ToString("N");
        using (var scope = Server!.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AnduinOSHomeDbContext>();
            (await db.Hardware.SingleAsync(x => x.Id == draft.Id)).Sku = needle;
            await db.SaveChangesAsync();
        }
        var html = await Http.GetStringAsync($"/ManageHardware?search={needle}&publication=Draft");
        Assert.Contains(needle, WebUtility.HtmlDecode(html));
        Assert.Contains($"/ManageHardware/Preview/{draft.Id}", html);
        Assert.DoesNotContain(published.Slug, html);
        Assert.AreEqual(HttpStatusCode.BadRequest,
            (await Http.GetAsync("/ManageHardware?publication=999")).StatusCode);
    }

    private async Task<int> DbId(string slug)
    {
        using var scope = Server!.Services.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<AnduinOSHomeDbContext>().Hardware
            .Where(x => x.Slug == slug).Select(x => x.Id).SingleAsync();
    }

    [TestMethod]
    public async Task DeviceFactsEditorDoesNotRequireOrOverwriteTranslationText()
    {
        await LoginAsAdmin();
        var id = await AddDevice(HardwarePublication.Published);
        var original = await Read(id);
        var html = await Http.GetStringAsync($"/ManageHardware/Edit/{id}");
        Assert.Contains($"/ManageHardware/Localize/{id}", html);
        Assert.Contains("name=\"Text.Description\"", html);
        Assert.DoesNotContain("culture-switch", html);

        var form = Form(id, original.Slug);
        form.Remove("Text.Culture");
        form.Remove("Text.Description");
        form["Device.Model"] = "Updated workstation";
        var response = await PostForm($"/ManageHardware/Edit/{id}", form, $"/ManageHardware/Edit/{id}");
        Assert.AreEqual(HttpStatusCode.Found, response.StatusCode, await response.Content.ReadAsStringAsync());
        var after = await Read(id);
        Assert.AreEqual("Updated workstation", after.Model);
        Assert.AreEqual("English device description", HardwareCatalog.TextFor(after, "en").Description);
        Assert.AreEqual("Deutsche Beschreibung", HardwareCatalog.TextFor(after, "de").Description);
    }

    [TestMethod]
    public async Task AnonymousAndOrdinaryUsersCannotManageHardware()
    {
        Assert.AreEqual(HttpStatusCode.Found, (await Http.GetAsync("/ManageHardware/Edit")).StatusCode);
        var draftId = await AddDevice(HardwarePublication.Draft);
        Assert.AreEqual(HttpStatusCode.Found,
            (await Http.GetAsync($"/ManageHardware/Preview/{draftId}")).StatusCode);
        await RegisterAndLoginAsync();
        var response = await Http.GetAsync("/ManageHardware/Edit");
        Assert.IsTrue(response.StatusCode is HttpStatusCode.Found or HttpStatusCode.Forbidden);
        response = await Http.GetAsync($"/ManageHardware/Preview/{draftId}");
        Assert.IsTrue(response.StatusCode is HttpStatusCode.Found or HttpStatusCode.Forbidden);
    }

    [TestMethod]
    public async Task RejectsUnsafePathsUrlsEnumsAndMissingSourceDescription()
    {
        await LoginAsAdmin();
        foreach (var (key, value) in new[]
        {
            ("Device.ProductImagePath", "avatar/test.png"),
            ("Device.ProductImagePath", "hardware/../../secret.png"),
            ("Device.ProductImagePath", "hardware/missing.png"),
            ("Device.ProductUrl", "javascript:alert(1)"),
            ("Device.Publication", "99"),
            ("Device.DeviceType", "99"),
            ("Device.Sku", new string('x', 101)),
            ("Text.VirtualizationDetail", new string('x', 2001)),
            ("Text.ConfigurationText", new string('x', 1001)),
            ("Text.ImageCreditText", new string('x', 501)),
            ("Device.Installation", "99"),
            ("Device.PriceUsd", "-1"),
            ("Device.SourceCulture", " ")
        })
        {
            var slug = Guid.NewGuid().ToString("N");
            var form = Form(0, slug);
            form[key] = value;
            var response = await PostForm("/ManageHardware/Edit/0", form, "/ManageHardware/Edit");
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(HttpStatusCode.NotFound, (await Http.GetAsync("/hardware/" + slug)).StatusCode);
        }
        var nonEnglish = Form(0, Guid.NewGuid().ToString("N"), "de");
        nonEnglish["Device.SourceCulture"] = "de";
        var saved = await PostForm("/ManageHardware/Edit/0", nonEnglish, "/ManageHardware/Edit");
        Assert.AreEqual(HttpStatusCode.Found, saved.StatusCode);
        var missingSource = Form(0, Guid.NewGuid().ToString("N"), "de");
        missingSource["Device.SourceCulture"] = "de";
        missingSource["Text.Description"] = "";
        var missingResponse = await PostForm("/ManageHardware/Edit/0", missingSource, "/ManageHardware/Edit");
        Assert.AreEqual(HttpStatusCode.OK, missingResponse.StatusCode);
        Assert.Contains("Write a source description before publishing.", await missingResponse.Content.ReadAsStringAsync());
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
            Sku = "EXAMPLE-001", DeviceType = HardwareDeviceType.Desktop,
            Translations = [new HardwareTranslation { Culture = "en", Description = "Test", VirtualizationDetail = "KVM works",
                ConfigurationText = "20-core ARM CPU", ImageCreditText = "Vendor photo" }] };
        db.Hardware.Add(item);
        await db.SaveChangesAsync();
        var persisted = await db.Hardware.AsNoTracking().SingleAsync(x => x.Id == item.Id);
        Assert.AreEqual("EXAMPLE-001", persisted.Sku);
        Assert.AreEqual(HardwareDeviceType.Desktop, persisted.DeviceType);
        Assert.AreEqual("KVM works", (await db.HardwareTranslations.AsNoTracking().SingleAsync(x => x.HardwareId == item.Id)).VirtualizationDetail);
        Assert.AreEqual("20-core ARM CPU", (await db.HardwareTranslations.AsNoTracking().SingleAsync(x => x.HardwareId == item.Id)).ConfigurationText);
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
        device.SourceCulture = "zh-CN";
        device.Translations.Add(new HardwareTranslation { Culture = "zh-CN", Description = "中文原文" });
        device.Translations.RemoveAll(x => x.Culture == "en");
        device.Translations.Add(new HardwareTranslation { Culture = "en-US", Description = "US English" });
        Assert.AreEqual("US English", HardwareCatalog.TextFor(device, "en-GB").Description);
        Assert.AreEqual("中文原文", HardwareCatalog.TextFor(device, "ja-JP").Description);
    }
}
