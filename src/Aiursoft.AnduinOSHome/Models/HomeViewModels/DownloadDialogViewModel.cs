using Aiursoft.AnduinOSHome.Configuration;
using Microsoft.AspNetCore.Mvc.Localization;

namespace Aiursoft.AnduinOSHome.Models.HomeViewModels;

public record DownloadDialogViewModel(
    string Id,
    string Version,
    IReadOnlyDictionary<string, ReleaseArtifact> Artifacts,
    IViewLocalizer Localizer,
    bool IncludeContainer,
    bool ShowNonLtsWarning);
