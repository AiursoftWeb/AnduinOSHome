using System.Net;
using Aiursoft.AnduinOSHome.Attributes;
using Aiursoft.AnduinOSHome.Entities;
using Aiursoft.AnduinOSHome.Models.RatingViewModels;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.AnduinOSHome.Tests.IntegrationTests;

[TestClass]
public class RatingControllerTests : TestBase
{
    [TestMethod]
    public async Task TestSubmitRating5StarsWithoutComment()
    {
        var response = await Http.PostAsJsonAsync("/api/rating", new RatingAddressModel
        {
            Stars = 5,
            Comment = null
        });

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.IsTrue(body.Contains("Thank you for your rating!", StringComparison.OrdinalIgnoreCase));

        using var scope = Server!.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AnduinOSHomeDbContext>();
        var rating = await db.OobeRatings.FirstOrDefaultAsync(r => r.Stars == 5);
        Assert.IsNotNull(rating);
        Assert.IsNull(rating.Comment);
        Assert.IsTrue(rating.SubmitTime > DateTime.UtcNow.AddMinutes(-5));
    }

    [TestMethod]
    public async Task TestSubmitRatingWithComment()
    {
        var response = await Http.PostAsJsonAsync("/api/rating", new RatingAddressModel
        {
            Stars = 3,
            Comment = "Need better touchpad gestures and scaling options"
        });

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        using var scope = Server!.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AnduinOSHomeDbContext>();
        var rating = await db.OobeRatings.FirstOrDefaultAsync(r => r.Stars == 3);
        Assert.IsNotNull(rating);
        Assert.AreEqual("Need better touchpad gestures and scaling options", rating.Comment);
    }

    [TestMethod]
    public async Task TestSubmitRatingInvalidStars()
    {
        var responseZero = await Http.PostAsJsonAsync("/api/rating", new RatingAddressModel
        {
            Stars = 0
        });
        Assert.AreEqual(HttpStatusCode.BadRequest, responseZero.StatusCode);

        var responseSix = await Http.PostAsJsonAsync("/api/rating", new RatingAddressModel
        {
            Stars = 6
        });
        Assert.AreEqual(HttpStatusCode.BadRequest, responseSix.StatusCode);
    }

    [TestMethod]
    public async Task TestSubmitRatingCommentTooLong()
    {
        var response = await Http.PostAsJsonAsync("/api/rating", new RatingAddressModel
        {
            Stars = 4,
            Comment = new string('x', 2001)
        });
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public async Task TestRateLimitPerHour()
    {
        LimitPerHour.KeepFunctioningInUnitTest = true;
        try
        {
            // First 3 requests must succeed
            for (var i = 1; i <= 3; i++)
            {
                var response = await Http.PostAsJsonAsync("/api/rating", new RatingAddressModel
                {
                    Stars = 5
                });
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, $"Request #{i} should succeed.");
                Assert.IsTrue(response.Headers.Contains("x-rate-limit-remaining"));
            }

            // 4th request must be rejected with 429 Too Many Requests
            var fourthResponse = await Http.PostAsJsonAsync("/api/rating", new RatingAddressModel
            {
                Stars = 5
            });
            Assert.AreEqual((HttpStatusCode)429, fourthResponse.StatusCode, "4th request within 1 hour should return 429.");
            Assert.IsTrue(fourthResponse.Headers.Contains("Retry-After"));
            Assert.AreEqual("3600", fourthResponse.Headers.GetValues("Retry-After").First());
        }
        finally
        {
            LimitPerHour.KeepFunctioningInUnitTest = false;
        }
    }
}
