using Aiursoft.UiStack.Layout;

namespace Aiursoft.AnduinOSHome.Models.HomeViewModels;

public class IndexViewModel : UiStackLayoutViewModel
{
    public IndexViewModel()
    {
        PageTitle = "Open Source & Linux";
    }

    public string Amd64ChecksumUrl { get; init; } = string.Empty;
    public string Arm64ChecksumUrl { get; init; } = string.Empty;
}
