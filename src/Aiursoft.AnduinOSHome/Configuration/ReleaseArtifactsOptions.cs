namespace Aiursoft.AnduinOSHome.Configuration;

public sealed class ReleaseArtifactsOptions
{
    public const string SectionName = "ReleaseArtifacts";
    public const string FastTrackChannel = "fast-track";

    public Dictionary<string, ReleaseArtifact> Architectures { get; init; } =
        new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string, ReleaseArtifact> FastTrackArchitectures { get; init; } =
        new(StringComparer.OrdinalIgnoreCase);

    public bool IsValid()
    {
        return IsValidRelease(Architectures) && IsValidRelease(FastTrackArchitectures);
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
