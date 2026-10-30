using System.ComponentModel.DataAnnotations;

namespace Aiursoft.AnduinOSHome.Entities;

public class OobeRating
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Range(1, 5)]
    public int Stars { get; set; }

    [MaxLength(2000)]
    public string? Comment { get; set; }

    public DateTime SubmitTime { get; set; } = DateTime.UtcNow;
}
