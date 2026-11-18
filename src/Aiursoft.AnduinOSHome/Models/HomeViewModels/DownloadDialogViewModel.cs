using Aiursoft.AnduinOSHome.Configuration;

namespace Aiursoft.AnduinOSHome.Models.HomeViewModels;

public record DownloadDialogViewModel(
    string Id,
    string Version,
    IReadOnlyDictionary<string, ReleaseArtifact> Artifacts,
    bool IncludeContainer,
    bool ShowNonLtsWarning);
