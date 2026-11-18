using Aiursoft.AnduinOSHome.Models.HomeViewModels;

namespace Aiursoft.AnduinOSHome.Configuration;

public sealed class ReleaseArtifactsOptions
{
    public const string SectionName = "ReleaseArtifacts";
    // The configuration binder populates the versions from appsettings.
    // ReSharper disable once CollectionNeverUpdated.Global
    public Dictionary<string, Dictionary<string, ReleaseArtifact>> Versions { get; init; } =
        new(StringComparer.Ordinal);

    public bool IsValid()
    {
        return Versions.ContainsKey(VersionData.Resolute.LatestPatch) &&
               Versions.ContainsKey(VersionData.Stonking.LatestPatch) &&
               Versions.Values.All(IsValidRelease);
    }

    private static bool IsValidRelease(Dictionary<string, ReleaseArtifact> architectures)
    {
        string[] requiredArchitectures = ["amd64", "arm64"];
        return requiredArchitectures.All(architectures.ContainsKey) &&
               architectures.Values.All(artifact => artifact.HasValidUrls());
    }
}

public sealed class ReleaseArtifact
{
    public string IsoUrl { get; init; } = string.Empty;
    public string TorrentUrl { get; init; } = string.Empty;
    public string ChecksumUrl { get; init; } = string.Empty;
    public string IsoSize { get; init; } = string.Empty;

    public string IsoFileName => Uri.TryCreate(IsoUrl, UriKind.Absolute, out var uri)
        ? Path.GetFileName(uri.LocalPath)
        : string.Empty;

    public bool HasValidUrls()
    {
        return IsHttpsArtifactUrl(IsoUrl, ".iso") &&
               IsHttpsArtifactUrl(TorrentUrl, ".torrent") &&
               IsHttpsArtifactUrl(ChecksumUrl, ".sha256") &&
               !string.IsNullOrWhiteSpace(IsoFileName);
    }

    private static bool IsHttpsArtifactUrl(string url, string expectedExtension)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
               uri.Scheme == Uri.UriSchemeHttps &&
               Path.GetExtension(uri.LocalPath).Equals(expectedExtension, StringComparison.OrdinalIgnoreCase);
    }
}
