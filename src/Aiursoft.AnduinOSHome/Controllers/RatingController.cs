using Aiursoft.AnduinOSHome.Attributes;
using Aiursoft.AnduinOSHome.Entities;
using Aiursoft.AnduinOSHome.Models.RatingViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Aiursoft.AnduinOSHome.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RatingController(
    AnduinOSHomeDbContext dbContext,
    ILogger<RatingController> logger) : ControllerBase
{
    [HttpPost]
    [LimitPerHour]
    public async Task<IActionResult> Post([FromBody] RatingAddressModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var rating = new OobeRating
        {
            Stars = model.Stars,
            Comment = string.IsNullOrWhiteSpace(model.Comment) ? null : model.Comment.Trim(),
            SubmitTime = DateTime.UtcNow
        };

        dbContext.OobeRatings.Add(rating);
        await dbContext.SaveChangesAsync();

        logger.LogInformation("Received anonymous OOBE rating: {Stars} stars", model.Stars);

        return Ok(new
        {
            message = "Thank you for your rating!"
        });
    }
}
