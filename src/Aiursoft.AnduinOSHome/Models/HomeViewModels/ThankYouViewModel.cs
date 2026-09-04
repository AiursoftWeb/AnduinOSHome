using Aiursoft.UiStack.Layout;

namespace Aiursoft.AnduinOSHome.Models.HomeViewModels;

public class ThankYouViewModel : UiStackLayoutViewModel
{
    public ThankYouViewModel()
    {
        PageTitle = "Thank you for downloading AnduinOS!";
    }

    public string DownloadUrl { get; init; } = string.Empty;
    public string ChecksumUrl { get; init; } = string.Empty;
    public string IsoFileName { get; init; } = string.Empty;
    public bool IsTorrent { get; init; }
}
