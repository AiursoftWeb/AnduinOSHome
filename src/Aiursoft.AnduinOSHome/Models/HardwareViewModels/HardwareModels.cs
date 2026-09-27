using System.ComponentModel.DataAnnotations;
using Aiursoft.AnduinOSHome.Entities;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.AnduinOSHome.Models.HardwareViewModels;

public class HardwareIndexModel : UiStackLayoutViewModel
{
    public int Page { get; set; } = 1;
    public bool HasNextPage { get; set; }
    public string? Search { get; set; }
    public HardwareDeviceType? DeviceType { get; set; }
    public string? Architecture { get; set; }
    public bool TeamOnly { get; set; }
    public HardwarePublication? PublicationFilter { get; set; }
    public List<string> Architectures { get; set; } = [];
    public HardwareIndexModel() { PageTitle = "Recommended hardware"; }
    public List<Hardware> Items { get; set; } = [];
}
public class HardwareDetailsModel : UiStackLayoutViewModel
{
    public HardwareDetailsModel() { PageTitle = "Hardware compatibility"; }
    public Hardware Device { get; set; } = new();
    public HardwareTranslation Text { get; set; } = new();
    public bool IsPreview { get; set; }
    public string? Configuration => Detail(x => x.ConfigurationText) ?? Device.Configuration;
    public string? ImageCredit => Detail(x => x.ImageCreditText) ?? Device.ImageCredit;

    public string? Detail(Func<HardwareTranslation, string?> select)
    {
        var localized = select(Text);
        if (!string.IsNullOrWhiteSpace(localized)) return localized;
        var source = Device.Translations.FirstOrDefault(x => x.Culture == Device.SourceCulture);
        var fallback = source == null ? null : select(source);
        return string.IsNullOrWhiteSpace(fallback) ? null : fallback;
    }
}

public record HardwareInsightModel(string Title, string Status, string Icon, Enum Verdict,
    string? Detail, bool Highlight);
public class HardwareEditModel : UiStackLayoutViewModel
{
    public bool ClearProductImage { get; set; }
    public bool ClearExperienceImage { get; set; }
    public HardwareEditModel() { PageTitle = "Manage hardware"; }
    public Hardware Device { get; set; } = new();
    public HardwareTranslation Text { get; set; } = new();
    public bool SourceCultureLocked { get; set; }
}

public record HardwareLanguage(string Culture, string NativeName, bool HasTranslation, bool IsStale);

public class HardwareLocalizeModel : UiStackLayoutViewModel
{
    public HardwareLocalizeModel() { PageTitle = "Hardware translations"; }
    public int DeviceId { get; set; }
    public string DeviceName { get; set; } = string.Empty;
    public string SourceCulture { get; set; } = "en";
    public bool IsPublished { get; set; }
    public List<HardwareLanguage> Languages { get; set; } = [];
}

public class HardwareTranslationInput
{
    [Required]
    [MaxLength(20)]
    public string Culture { get; set; } = string.Empty;
    [Required]
    [MaxLength(3000)]
    public string Description { get; set; } = string.Empty;
    [MaxLength(3000)] public string? InstallationNotes { get; set; }
    [MaxLength(3000)] public string? FirmwareNotes { get; set; }
    [MaxLength(3000)] public string? KnownIssues { get; set; }
    [MaxLength(2000)] public string? InstallationDetail { get; set; }
    [MaxLength(2000)] public string? PerformanceDetail { get; set; }
    [MaxLength(2000)] public string? SecureBootDetail { get; set; }
    [MaxLength(2000)] public string? WifiDetail { get; set; }
    [MaxLength(2000)] public string? GraphicsDetail { get; set; }
    [MaxLength(2000)] public string? VirtualizationDetail { get; set; }
    [MaxLength(2000)] public string? DisplayDetail { get; set; }
    [MaxLength(1000)] public string? ConfigurationText { get; set; }
    [MaxLength(500)] public string? ImageCreditText { get; set; }
}
