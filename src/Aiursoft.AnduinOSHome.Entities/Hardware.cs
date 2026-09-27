using System.ComponentModel.DataAnnotations;

namespace Aiursoft.AnduinOSHome.Entities;

public enum HardwarePublication { Draft, Published, Archived }
public enum HardwareEase { Untested, Unsupported, Difficult, Straightforward }
public enum HardwarePerformance { Untested, Insufficient, Adequate, Ideal }
public enum HardwareSupport { Untested, Unsupported, Supported, NotApplicable }
public enum HardwareGraphics { Untested, Unsupported, LiveReady, AutomaticInstallation, ManualSetup }
public enum HardwareDeviceType
{
    Unspecified, HomeSupercomputer, Laptop, Desktop, Server, DevelopmentBoard,
    Tablet, Phone, Television, MiniPc, Workstation, Other
}

public class Hardware
{
    public int Id { get; set; }
    [Required, MaxLength(100), RegularExpression(@"^[a-z0-9]+(?:-[a-z0-9]+)*$")]
    public string Slug { get; set; } = string.Empty;
    [Required, MaxLength(100)] public string Brand { get; set; } = string.Empty;
    [Required, MaxLength(150)] public string Model { get; set; } = string.Empty;
    [MaxLength(100)] public string? Sku { get; set; }
    [Required, MaxLength(30)] public string Architecture { get; set; } = string.Empty;
    [MaxLength(100)] public string? Category { get; set; }
    [EnumDataType(typeof(HardwareDeviceType))] public HardwareDeviceType DeviceType { get; set; }
    [MaxLength(1000)] public string? Configuration { get; set; }
    [Range(0, int.MaxValue)] public int? PriceUsd { get; set; }
    [MaxLength(100)] public string? PriceMarket { get; set; }
    public DateTime? PriceCheckedAt { get; set; }
    [MaxLength(1000)] public string? ProductUrl { get; set; }
    [MaxLength(1000)] public string? ReportUrl { get; set; }
    [MaxLength(250)] public string? ProductImagePath { get; set; }
    [MaxLength(250)] public string? ExperienceImagePath { get; set; }
    [MaxLength(1000)] public string? ImageSourceUrl { get; set; }
    [MaxLength(500)] public string? ImageCredit { get; set; }
    [MaxLength(100)] public string? TestedVersion { get; set; }
    [MaxLength(100)] public string? KernelVersion { get; set; }
    [MaxLength(100)] public string? DriverVersion { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public bool TeamDevice { get; set; }
    [EnumDataType(typeof(HardwareEase))] public HardwareEase Installation { get; set; }
    [EnumDataType(typeof(HardwareEase))] public HardwareEase Virtualization { get; set; }
    [EnumDataType(typeof(HardwarePerformance))] public HardwarePerformance Performance { get; set; }
    [EnumDataType(typeof(HardwareSupport))] public HardwareSupport Wifi { get; set; }
    [EnumDataType(typeof(HardwareSupport))] public HardwareSupport Display { get; set; }
    [EnumDataType(typeof(HardwareSupport))] public HardwareSupport SecureBoot { get; set; }
    [EnumDataType(typeof(HardwareGraphics))] public HardwareGraphics Graphics { get; set; }
    [EnumDataType(typeof(HardwarePublication))] public HardwarePublication Publication { get; set; }
    public bool Featured { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public List<HardwareTranslation> Translations { get; set; } = [];
}

public class HardwareTranslation
{
    public int Id { get; set; }
    public int HardwareId { get; set; }
    [Required, MaxLength(20)] public string Culture { get; set; } = "en";
    [Required, MaxLength(3000)] public string Description { get; set; } = string.Empty;
    [MaxLength(3000)] public string? FirmwareNotes { get; set; }
    [MaxLength(3000)] public string? InstallationNotes { get; set; }
    [MaxLength(3000)] public string? KnownIssues { get; set; }
    [MaxLength(2000)] public string? InstallationDetail { get; set; }
    [MaxLength(2000)] public string? PerformanceDetail { get; set; }
    [MaxLength(2000)] public string? SecureBootDetail { get; set; }
    [MaxLength(2000)] public string? WifiDetail { get; set; }
    [MaxLength(2000)] public string? GraphicsDetail { get; set; }
    [MaxLength(2000)] public string? VirtualizationDetail { get; set; }
    [MaxLength(2000)] public string? DisplayDetail { get; set; }
    [MaxLength(1000)] public string? ConfigurationText { get; set; }
    [MaxLength(500)] public string? ImageCreditText { get; set; }
}
