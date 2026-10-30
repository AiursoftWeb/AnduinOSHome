using System.ComponentModel.DataAnnotations;

namespace Aiursoft.AnduinOSHome.Models.RatingViewModels;

public class RatingAddressModel
{
    [Range(1, 5, ErrorMessage = "Stars must be between 1 and 5.")]
    public int Stars { get; set; }

    [MaxLength(2000, ErrorMessage = "Comment cannot exceed 2000 characters.")]
    public string? Comment { get; set; }
}
