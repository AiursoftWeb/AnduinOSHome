using Aiursoft.AnduinOSHome.Entities;
using Aiursoft.Scanner.Abstractions;
using Microsoft.Extensions.Localization;

namespace Aiursoft.AnduinOSHome.Services;

public class HardwareLabels(IStringLocalizer<HardwareLabels> localizer) : ITransientDependency
{
    public static string StatusTone(Enum value) => value switch
    {
        HardwareEase.Straightforward or HardwarePerformance.Ideal or HardwareSupport.Supported
            or HardwareGraphics.LiveReady or HardwareGraphics.AutomaticInstallation => "positive",
        HardwareEase.Difficult or HardwarePerformance.Adequate or HardwareGraphics.ManualSetup => "caution",
        HardwareEase.Unsupported or HardwarePerformance.Insufficient or HardwareSupport.Unsupported
            or HardwareGraphics.Unsupported => "negative",
        _ => "unknown"
    };

    public static string StatusIcon(Enum value) => StatusTone(value) switch
    {
        "positive" => "check",
        "caution" or "negative" => "alert-triangle",
        _ => "minus"
    };

    public string Label(Enum value) => value switch
    {
        HardwarePublication.Draft => localizer["Draft"],
        HardwarePublication.Published => localizer["Published"],
        HardwarePublication.Archived => localizer["Archived"],
        HardwareEase.Unsupported => localizer["Not supported"],
        HardwareEase.Difficult => localizer["Requires additional setup"],
        HardwareEase.Straightforward => localizer["Straightforward"],
        HardwarePerformance.Insufficient => localizer["Below requirements"],
        HardwarePerformance.Adequate => localizer["Meets requirements"],
        HardwarePerformance.Ideal => localizer["Ideal for the AnduinOS desktop"],
        HardwareSupport.Unsupported => localizer["Not supported"],
        HardwareSupport.Supported => localizer["Verified"],
        HardwareSupport.NotApplicable => localizer["Not applicable"],
        HardwareGraphics.Unsupported => localizer["Not supported"],
        HardwareGraphics.LiveReady => localizer["Hardware acceleration available in Live"],
        HardwareGraphics.AutomaticInstallation => localizer["Drivers configured automatically during installation"],
        HardwareGraphics.ManualSetup => localizer["Manual driver setup required"],
        _ => localizer["Not tested"]
    };
}
