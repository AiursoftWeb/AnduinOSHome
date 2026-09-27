using System.Globalization;
using Aiursoft.AnduinOSHome.Authorization;
using Aiursoft.AnduinOSHome.Entities;
using Aiursoft.AnduinOSHome.Models.HardwareViewModels;
using Aiursoft.AnduinOSHome.Services;
using Aiursoft.AnduinOSHome.Services.FileStorage;
using Aiursoft.UiStack.Navigation;
using Aiursoft.WebTools.Attributes;
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
    public async Task<IActionResult> Index() => this.StackView(new HardwareIndexModel
    {
        Items = await db.Hardware.AsNoTracking().OrderBy(x => x.SortOrder).ThenBy(x => x.Id).ToListAsync()
    });

    [HttpGet]
    public async Task<IActionResult> Edit(int id = 0, string culture = "en")
    {
        var device = id == 0 ? new Hardware() : await db.Hardware.AsNoTracking()
            .Include(x => x.Translations).SingleOrDefaultAsync(x => x.Id == id);
        if (device == null) return NotFound();
        var normalized = NormalizeCulture(culture);
        if (normalized == null) return BadRequest();
        return this.StackView(new HardwareEditModel
        {
            Device = device,
            Text = device.Translations.SingleOrDefault(x => x.Culture == normalized)
                ?? new HardwareTranslation { Culture = normalized }
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

        var culture = NormalizeCulture(model.Text.Culture);
        if (culture == null) ModelState.AddModelError("Text.Culture", localizer["Enter a valid language code."]);
        else model.Text.Culture = culture;
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
        if (model.Device.Publication == HardwarePublication.Published &&
            culture != "en" && existing?.Translations.All(x => x.Culture != "en") != false)
            ModelState.AddModelError("Text.Culture", localizer["Save an English description before publishing."]);

        if (!ModelState.IsValid) return this.StackView(model);

        var device = existing ?? new Hardware();
        var created = device.CreatedAt;
        db.Entry(device).CurrentValues.SetValues(model.Device);
        device.CreatedAt = created;
        device.UpdatedAt = DateTime.UtcNow;
        var translation = device.Translations.SingleOrDefault(x => x.Culture == culture);
        if (translation == null)
        {
            translation = new HardwareTranslation { Culture = culture! };
            device.Translations.Add(translation);
        }
        translation.Description = model.Text.Description;
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
        if (existing == null) db.Hardware.Add(device);
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, localizer["Could not save this record. Check whether its address is already in use."]);
            return this.StackView(model);
        }
        return RedirectToAction(nameof(Edit), new { id = device.Id, culture });
    }

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
