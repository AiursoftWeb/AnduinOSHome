using System.ComponentModel.DataAnnotations;

namespace Aiursoft.AnduinOSHome.Models.OfflineUpdates;

public record PublishUpdate
{
    [Required, RegularExpression("[a-f0-9]{64}")] public string BatchId { get; set; } = string.Empty;
    [Required, RegularExpression("[0-9]{1,4}\\.[0-9]{1,4}")] public string ReleaseLine { get; set; } = string.Empty;
    [Required, RegularExpression("[a-z][a-z0-9-]{0,31}")] public string Suite { get; set; } = string.Empty;
    [Required, RegularExpression("amd64|arm64")] public string Architecture { get; set; } = string.Empty;
    [Required, RegularExpression("[0-9]{1,4}\\.[0-9]{1,4}\\.[0-9]{1,6}")] public string TargetVersion { get; set; } = string.Empty;
    [Required, RegularExpression("[0-9]{1,4}\\.[0-9]{1,4}\\.[0-9]{1,6}")] public string MinimumVersion { get; set; } = string.Empty;
    [Required, RegularExpression("[a-f0-9]{64}")] public string Sha256 { get; set; } = string.Empty;
    [Required, RegularExpression("([A-F0-9]{40}|[A-F0-9]{64})")] public string SigningFingerprint { get; set; } = string.Empty;
    [Range(1, 107374182400)] public long Size { get; set; }
    public DateTime CreatedUtc { get; set; }
    [Required, StringLength(2000)] public string Summary { get; set; } = string.Empty;
}
