using Aiursoft.UiStack.Layout;

namespace Aiursoft.AnduinOSHome.Models.OfflineUpdates;

public class OfflineUpdatesViewModel : UiStackLayoutViewModel
{
    public OfflineUpdatesViewModel() { PageTitle = "Offline updates"; }
    public bool Available { get; init; } = true;
    public IReadOnlyList<OfflineRelease> Releases { get; init; } = [];
}
