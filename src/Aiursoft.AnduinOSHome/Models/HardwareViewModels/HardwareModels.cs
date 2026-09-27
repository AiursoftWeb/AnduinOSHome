using Aiursoft.AnduinOSHome.Entities;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.AnduinOSHome.Models.HardwareViewModels;

public class HardwareIndexModel : UiStackLayoutViewModel
{
    public int Page { get; set; } = 1;
    public bool HasNextPage { get; set; }
    public HardwareIndexModel() { PageTitle = "Recommended hardware"; }
    public List<Hardware> Items { get; set; } = [];
}
public class HardwareDetailsModel : UiStackLayoutViewModel
{
    public HardwareDetailsModel() { PageTitle = "Hardware compatibility"; }
    public Hardware Device { get; set; } = new();
    public HardwareTranslation Text { get; set; } = new();
}
public class HardwareEditModel : UiStackLayoutViewModel
{
    public bool ClearProductImage { get; set; }
    public bool ClearExperienceImage { get; set; }
    public HardwareEditModel() { PageTitle = "Manage hardware"; }
    public Hardware Device { get; set; } = new();
    public HardwareTranslation Text { get; set; } = new();
}
