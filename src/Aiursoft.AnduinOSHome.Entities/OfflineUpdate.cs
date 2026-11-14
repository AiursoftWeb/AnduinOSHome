using System.ComponentModel.DataAnnotations;

namespace Aiursoft.AnduinOSHome.Entities;

public class OfflineUpdate
{
    [Key, MaxLength(64)] public required string BatchId { get; set; }
    [MaxLength(16)] public required string ReleaseLine { get; set; }
    [MaxLength(32)] public required string Suite { get; set; }
    [MaxLength(16)] public required string Architecture { get; set; }
    [MaxLength(32)] public required string TargetVersion { get; set; }
    public int TargetPatch { get; set; }
    [MaxLength(32)] public required string MinimumVersion { get; set; }
    [MaxLength(64)] public required string Sha256 { get; set; }
    [MaxLength(64)] public required string SigningFingerprint { get; set; }
    public long Size { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime PublishedUtc { get; set; }
    public bool Withdrawn { get; set; }
    [MaxLength(2000)] public required string Summary { get; set; }

    public string ObjectKey => $"offline/{Suite}/{ReleaseLine}/{Architecture}/{BatchId}/{Sha256}.atgz";
    public string FileName => $"AnduinOS-{TargetVersion}-{Architecture}-{BatchId[..12]}.atgz";
}
