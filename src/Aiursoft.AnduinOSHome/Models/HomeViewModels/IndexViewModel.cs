using Aiursoft.AnduinOSHome.Configuration;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.AnduinOSHome.Models.HomeViewModels;

public class IndexViewModel : UiStackLayoutViewModel
{
    public IndexViewModel()
    {
        PageTitle = "Open Source & Linux";
    }

    public required IReadOnlyDictionary<string, ReleaseArtifact> ResoluteArtifacts { get; init; }
    public required IReadOnlyDictionary<string, ReleaseArtifact> StonkingArtifacts { get; init; }
}
