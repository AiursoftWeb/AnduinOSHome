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
    public List<string> Architectures { get; set; } = [];
    public HardwareIndexModel() { PageTitle = "Recommended hardware"; }
    public List<Hardware> Items { get; set; } = [];
}
public class HardwareDetailsModel : UiStackLayoutViewModel
{
    public HardwareDetailsModel() { PageTitle = "Hardware compatibility"; }
    public Hardware Device { get; set; } = new();
    public HardwareTranslation Text { get; set; } = new();
    public string? Configuration => Detail(x => x.ConfigurationText) ?? Device.Configuration;
    public string? ImageCredit => Detail(x => x.ImageCreditText) ?? Device.ImageCredit;

    public string? Detail(Func<HardwareTranslation, string?> select)
    {
        var localized = select(Text);
        if (!string.IsNullOrWhiteSpace(localized)) return localized;
        var english = Device.Translations.FirstOrDefault(x => x.Culture == "en");
        var fallback = english == null ? null : select(english);
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
}
