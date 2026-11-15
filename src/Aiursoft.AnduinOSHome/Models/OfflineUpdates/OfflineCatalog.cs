using System.Text.Json.Serialization;

namespace Aiursoft.AnduinOSHome.Models.OfflineUpdates;

public sealed class OfflineCatalog
{
    public int SchemaVersion { get; init; }
    public List<OfflineRelease>? Releases { get; init; }
}

public sealed class OfflineRelease
{
    public required OfflinePackage Update { get; init; }
    public required string ObjectKey { get; init; }
    public DateTimeOffset PublishedUtc { get; init; }

    [JsonIgnore]
    public string DownloadUrl => $"https://offline-update-packages.anduinos.com/{ObjectKey}";
}

public sealed class OfflinePackage
{
    public required string BatchId { get; init; }
    public required string ReleaseLine { get; init; }
    public required string Suite { get; init; }
    public required string Architecture { get; init; }
    public required string TargetVersion { get; init; }
    public required string MinimumVersion { get; init; }
    public required string Sha256 { get; init; }
    public long Size { get; init; }
    public DateTimeOffset CreatedUtc { get; init; }
}
