using System.Globalization;
using Aiursoft.AnduinOSHome.Authorization;
using Aiursoft.AnduinOSHome.Entities;
using Aiursoft.AnduinOSHome.Models.HardwareViewModels;
using Aiursoft.AnduinOSHome.Services;
using Aiursoft.AnduinOSHome.Services.FileStorage;
using Aiursoft.UiStack.Navigation;
using Aiursoft.WebTools.Attributes;
using Aiursoft.WebTools.OfficialPlugins;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace Aiursoft.AnduinOSHome.Controllers;

[Authorize(Policy = AppPermissionNames.CanManageHardware)]
[LimitPerMin]
public class ManageHardwareController(
    AnduinOSHomeDbContext db, StorageService storage, ImageProcessingService images,
    IStringLocalizer<ManageHardwareController> localizer) : Controller
{
    // Only holders of CanManageHardware can issue upload grants or attach public
    // catalog assets. These are shared editorial assets, not user-private files.
    private const string ImageFolder = "hardware/";

    [RenderInNavBar(NavGroupName = "Administration", NavGroupOrder = 9999,
        CascadedLinksGroupName = "Hardware", CascadedLinksIcon = "monitor", CascadedLinksOrder = 20,
        LinkText = "Recommended hardware", LinkOrder = 1)]
    public async Task<IActionResult> Index(string? search = null, HardwarePublication? publication = null)
    {
        if (!ModelState.IsValid || search?.Length > 100 ||
            (publication.HasValue && !Enum.IsDefined(publication.Value))) return BadRequest();
        var query = db.Hardware.AsNoTracking().Include(x => x.Translations).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLowerInvariant();
            query = query.Where(x => (x.Brand + " " + x.Model).ToLower().Contains(term) ||
                (x.Sku != null && x.Sku.ToLower().Contains(term)));
        }
        if (publication.HasValue) query = query.Where(x => x.Publication == publication.Value);
        return this.StackView(new HardwareIndexModel
        {
            Search = search, PublicationFilter = publication,
            Items = await query.OrderBy(x => x.SortOrder).ThenBy(x => x.Id).ToListAsync()
        });
    }

    [HttpGet]
    public async Task<IActionResult> Preview(int id, string? culture = null)
    {
        var normalized = culture == null ? null : NormalizeCulture(culture);
        if (culture != null && normalized == null) return BadRequest();
        var device = await db.Hardware.AsNoTracking().Include(x => x.Translations)
            .SingleOrDefaultAsync(x => x.Id == id);
        if (device == null) return NotFound();
        return View("~/Views/Hardware/Details.cshtml", new HardwareDetailsModel
        {
            Device = device, Text = HardwareCatalog.TextFor(device, normalized ?? device.SourceCulture), IsPreview = true
        });
    }

    [HttpGet]
    public async Task<IActionResult> Localize(int id)
    {
        var device = await db.Hardware.AsNoTracking().Include(x => x.Translations)
            .SingleOrDefaultAsync(x => x.Id == id);
        if (device == null) return NotFound();

        var supported = LocalizationPlugin.SupportedCultures;
        var cultures = supported.Keys.Where(x =>
                !x.Equals(device.SourceCulture, StringComparison.OrdinalIgnoreCase) &&
                !(device.SourceCulture == "en" && x.Equals("en-US", StringComparison.OrdinalIgnoreCase)))
            .Concat(device.Translations.Select(x => x.Culture))
            .Where(x => !x.Equals(device.SourceCulture, StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase);
        return this.StackView(new HardwareLocalizeModel
        {
            DeviceId = id,
            DeviceName = $"{device.Brand} {device.Model}",
            SourceCulture = device.SourceCulture,
            IsPublished = device.Publication == HardwarePublication.Published,
            Languages = cultures.Select(code => new HardwareLanguage(code,
                CultureInfo.GetCultureInfo(code).NativeName,
                device.Translations.Any(x => x.Culture.Equals(code, StringComparison.OrdinalIgnoreCase)),
                device.Translations.Any(x => x.Culture.Equals(code, StringComparison.OrdinalIgnoreCase)
                    && x.BasedOnSourceRevision < device.SourceRevision)))
                .ToList()
        });
    }

    [HttpGet]
    public async Task<IActionResult> TranslationData(int id, string culture)
    {
        var normalized = NormalizeCulture(culture);
        if (normalized == null) return BadRequest();
        var device = await db.Hardware.AsNoTracking().Include(x => x.Translations)
            .SingleOrDefaultAsync(x => x.Id == id);
        if (device == null) return NotFound();
        if (normalized.Equals(device.SourceCulture, StringComparison.OrdinalIgnoreCase)) return BadRequest();
        var text = device.Translations.SingleOrDefault(x => x.Culture == normalized);
        var source = device.Translations.SingleOrDefault(x => x.Culture == device.SourceCulture);
        return Json(new
        {
            culture = normalized,
            hasTranslation = text != null,
            isStale = text != null && text.BasedOnSourceRevision < device.SourceRevision,
            source = new
            {
                description = source?.Description, installationNotes = source?.InstallationNotes,
                firmwareNotes = source?.FirmwareNotes, knownIssues = source?.KnownIssues,
                installationDetail = source?.InstallationDetail, performanceDetail = source?.PerformanceDetail,
                secureBootDetail = source?.SecureBootDetail, wifiDetail = source?.WifiDetail,
                graphicsDetail = source?.GraphicsDetail, virtualizationDetail = source?.VirtualizationDetail,
                displayDetail = source?.DisplayDetail, configurationText = source?.ConfigurationText,
                imageCreditText = source?.ImageCreditText
            },
            description = text?.Description ?? string.Empty,
            installationNotes = text?.InstallationNotes ?? string.Empty,
            firmwareNotes = text?.FirmwareNotes ?? string.Empty,
            knownIssues = text?.KnownIssues ?? string.Empty,
            installationDetail = text?.InstallationDetail ?? string.Empty,
            performanceDetail = text?.PerformanceDetail ?? string.Empty,
            secureBootDetail = text?.SecureBootDetail ?? string.Empty,
            wifiDetail = text?.WifiDetail ?? string.Empty,
            graphicsDetail = text?.GraphicsDetail ?? string.Empty,
            virtualizationDetail = text?.VirtualizationDetail ?? string.Empty,
            displayDetail = text?.DisplayDetail ?? string.Empty,
            configurationText = text?.ConfigurationText ?? string.Empty,
            imageCreditText = text?.ImageCreditText ?? string.Empty
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveTranslation(int id, [FromForm] HardwareTranslationInput input)
    {
        var culture = NormalizeCulture(input.Culture);
        if (culture == null || !ModelState.IsValid || string.IsNullOrWhiteSpace(input.Description))
            return BadRequest(new { error = localizer["Enter a valid language and description."].Value });
        var device = await db.Hardware.Include(x => x.Translations).SingleOrDefaultAsync(x => x.Id == id);
        if (device == null) return NotFound();
        if (culture.Equals(device.SourceCulture, StringComparison.OrdinalIgnoreCase)) return BadRequest();
        var text = device.Translations.SingleOrDefault(x => x.Culture == culture);
        if (text == null)
        {
            text = new HardwareTranslation { Culture = culture };
            device.Translations.Add(text);
        }
        text.Description = input.Description;
        text.InstallationNotes = input.InstallationNotes;
        text.FirmwareNotes = input.FirmwareNotes;
        text.KnownIssues = input.KnownIssues;
        text.InstallationDetail = input.InstallationDetail;
        text.PerformanceDetail = input.PerformanceDetail;
        text.SecureBootDetail = input.SecureBootDetail;
        text.WifiDetail = input.WifiDetail;
        text.GraphicsDetail = input.GraphicsDetail;
        text.VirtualizationDetail = input.VirtualizationDetail;
        text.DisplayDetail = input.DisplayDetail;
        text.ConfigurationText = input.ConfigurationText;
        text.ImageCreditText = input.ImageCreditText;
        text.BasedOnSourceRevision = device.SourceRevision;
        await db.SaveChangesAsync();
        return Json(new { success = true, culture });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id = 0)
    {
        var device = id == 0 ? new Hardware() : await db.Hardware.AsNoTracking()
            .Include(x => x.Translations).SingleOrDefaultAsync(x => x.Id == id);
        if (device == null) return NotFound();
        if (id == 0) device.SourceCulture = NormalizeCulture(CultureInfo.CurrentUICulture.Name) ?? "en";
        return this.StackView(new HardwareEditModel
        {
            Device = device,
            Text = device.Translations.SingleOrDefault(x => x.Culture == device.SourceCulture)
                ?? new HardwareTranslation { Culture = device.SourceCulture },
            SourceCultureLocked = id != 0
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, HardwareEditModel model)
    {
        if (id != model.Device.Id) return BadRequest();
        var existing = id == 0 ? null : await db.Hardware.Include(x => x.Translations)
            .SingleOrDefaultAsync(x => x.Id == id);
        if (id != 0 && existing == null) return NotFound();

        var savingTranslation = Request.HasFormContentType && Request.Form.ContainsKey("Text.Description");
        if (!savingTranslation || (model.Device.Publication != HardwarePublication.Published &&
                                   string.IsNullOrWhiteSpace(model.Text.Description)))
            ModelState.Remove("Text.Description");

        var culture = NormalizeCulture(model.Device.SourceCulture);
        if (culture == null) ModelState.AddModelError("Device.SourceCulture", localizer["Enter a valid language code."]);
        else model.Device.SourceCulture = culture;
        if (existing != null && !existing.SourceCulture.Equals(culture, StringComparison.OrdinalIgnoreCase))
            ModelState.AddModelError("Device.SourceCulture", localizer["The source language cannot be changed after creation."]);
        model.Text.Culture = existing?.SourceCulture ?? culture ?? string.Empty;
        if (model.ClearProductImage) model.Device.ProductImagePath = null;
        if (model.ClearExperienceImage) model.Device.ExperienceImagePath = null;

        foreach (var (key, url) in new[]
        {
            ("Device.ProductUrl", model.Device.ProductUrl),
            ("Device.ReportUrl", model.Device.ReportUrl),
            ("Device.ImageSourceUrl", model.Device.ImageSourceUrl)
        })
        {
            if (!string.IsNullOrWhiteSpace(url) && (!Uri.TryCreate(url, UriKind.Absolute, out var uri)
                || uri.Scheme != Uri.UriSchemeHttps || !string.IsNullOrEmpty(uri.UserInfo)))
                ModelState.AddModelError(key, localizer["Use an absolute HTTPS URL without credentials."]);
        }
        if (await db.Hardware.AnyAsync(x => x.Slug == model.Device.Slug && x.Id != id))
            ModelState.AddModelError("Device.Slug", localizer["This address is already in use."]);

        await ValidateImage("Device.ProductImagePath", model.Device.ProductImagePath);
        await ValidateImage("Device.ExperienceImagePath", model.Device.ExperienceImagePath);
        var sourceDescription = savingTranslation ? model.Text.Description : existing?.Translations
            .SingleOrDefault(x => x.Culture == existing.SourceCulture)?.Description;
        if (model.Device.Publication == HardwarePublication.Published && string.IsNullOrWhiteSpace(sourceDescription))
            ModelState.AddModelError("Text.Description", localizer["Write a source description before publishing."]);

        if (!ModelState.IsValid) { model.SourceCultureLocked = existing != null; return this.StackView(model); }

        var device = existing ?? new Hardware();
        var created = device.CreatedAt;
        var sourceRevision = device.SourceRevision;
        db.Entry(device).CurrentValues.SetValues(model.Device);
        if (existing == null) device.SourceCulture = culture!;
        device.SourceRevision = sourceRevision;
        device.CreatedAt = created;
        device.UpdatedAt = DateTime.UtcNow;
        if (savingTranslation)
        {
            var translation = device.Translations.SingleOrDefault(x => x.Culture == device.SourceCulture);
            if (translation == null)
            {
                translation = new HardwareTranslation { Culture = device.SourceCulture };
                device.Translations.Add(translation);
            }
            var changed = !SameText(translation, model.Text);
            translation.Description = string.IsNullOrEmpty(model.Text.Description)
                ? string.Empty : model.Text.Description;
            translation.FirmwareNotes = model.Text.FirmwareNotes ?? string.Empty;
            translation.InstallationNotes = model.Text.InstallationNotes ?? string.Empty;
            translation.KnownIssues = model.Text.KnownIssues ?? string.Empty;
            translation.InstallationDetail = model.Text.InstallationDetail;
            translation.PerformanceDetail = model.Text.PerformanceDetail;
            translation.SecureBootDetail = model.Text.SecureBootDetail;
            translation.WifiDetail = model.Text.WifiDetail;
            translation.GraphicsDetail = model.Text.GraphicsDetail;
            translation.VirtualizationDetail = model.Text.VirtualizationDetail;
            translation.DisplayDetail = model.Text.DisplayDetail;
            translation.ConfigurationText = model.Text.ConfigurationText;
            translation.ImageCreditText = model.Text.ImageCreditText;
            if (changed) device.SourceRevision++;
            translation.BasedOnSourceRevision = device.SourceRevision;
        }
        if (existing == null) db.Hardware.Add(device);
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, localizer["Could not save this record. Check whether its address is already in use."]);
            return this.StackView(model);
        }
        return RedirectToAction(nameof(Edit), new { id = device.Id });
    }

    private static bool SameText(HardwareTranslation a, HardwareTranslation b) =>
        new[] { a.Description, a.InstallationNotes, a.FirmwareNotes, a.KnownIssues,
            a.InstallationDetail, a.PerformanceDetail, a.SecureBootDetail, a.WifiDetail,
            a.GraphicsDetail, a.VirtualizationDetail, a.DisplayDetail, a.ConfigurationText,
            a.ImageCreditText }.Select(x => x ?? string.Empty).SequenceEqual(new[] { b.Description, b.InstallationNotes,
            b.FirmwareNotes, b.KnownIssues, b.InstallationDetail, b.PerformanceDetail,
            b.SecureBootDetail, b.WifiDetail, b.GraphicsDetail, b.VirtualizationDetail,
            b.DisplayDetail, b.ConfigurationText, b.ImageCreditText }.Select(x => x ?? string.Empty), StringComparer.Ordinal);

    private async Task ValidateImage(string key, string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) return;
        try
        {
            if (!path.StartsWith(ImageFolder, StringComparison.Ordinal) || path.Contains("..") || path.Contains('\\'))
                throw new ArgumentException();
            var physical = storage.GetFilePhysicalPath(path);
            if (!System.IO.File.Exists(physical) || new FileInfo(physical).Length > 10 * 1024 * 1024 ||
                !new[] { ".png", ".jpg", ".jpeg", ".webp" }.Contains(Path.GetExtension(path).ToLowerInvariant()) ||
                !await images.IsValidImageAsync(physical))
                throw new ArgumentException();
        }
        catch (ArgumentException)
        {
            ModelState.AddModelError(key, localizer["Select a valid public hardware image uploaded through this form."]);
        }
    }

    private static string? NormalizeCulture(string? culture)
    {
        if (string.IsNullOrWhiteSpace(culture) || culture.Length > 20) return null;
        try
        {
            var info = CultureInfo.GetCultureInfo(culture);
            return string.IsNullOrEmpty(info.Name) || info.ThreeLetterISOLanguageName == "ZZZ" ? null : info.Name;
        }
        catch (CultureNotFoundException) { return null; }
    }
}
