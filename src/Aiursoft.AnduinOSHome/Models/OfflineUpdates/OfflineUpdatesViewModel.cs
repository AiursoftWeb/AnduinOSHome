using Aiursoft.AnduinOSHome.Entities;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.AnduinOSHome.Models.OfflineUpdates;

public class OfflineUpdatesViewModel : UiStackLayoutViewModel
{
    public OfflineUpdatesViewModel() { PageTitle = "Offline updates"; }
    public IReadOnlyList<OfflineUpdate> Updates { get; init; } = [];
}
