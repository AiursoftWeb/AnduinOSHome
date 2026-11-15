using System.Net;
using System.Security.Claims;
using Aiursoft.AnduinOSHome.Authorization;
using Aiursoft.AnduinOSHome.Controllers;
using Aiursoft.AnduinOSHome.Entities;
using Aiursoft.AnduinOSHome.Models.OobeFeedbackViewModels;
using Aiursoft.AnduinOSHome.Sqlite;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.AnduinOSHome.Tests.IntegrationTests;

[TestClass]
public class OobeFeedbackTests : TestBase
{
    [TestMethod]
    public async Task RequiresDedicatedPermissionAndHidesNavigation()
    {
        var anonymous = await Http.GetAsync("/OobeFeedback");
        Assert.AreEqual(HttpStatusCode.Found, anonymous.StatusCode);
        Assert.Contains("/Account/Login", anonymous.Headers.Location!.OriginalString);
        var (email, password) = await RegisterAndLoginAsync();
        var denied = await Http.GetAsync("/OobeFeedback?from=2026-11-01&to=2026-11-30");
        Assert.AreEqual(HttpStatusCode.Found, denied.StatusCode);
        Assert.Contains("/Error/Code403", denied.Headers.Location!.OriginalString);
        Assert.DoesNotContain("/OobeFeedback", await Http.GetStringAsync("/Dashboard"));

        using (var scope = Server!.Services.CreateScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var user = await users.FindByEmailAsync(email);
            Assert.IsTrue((await users.AddClaimAsync(user!, new Claim(AppPermissions.Type,
                AppPermissionNames.ViewOobeFeedbackData))).Succeeded);
        }
        await PostForm("/Account/Logoff", []);
        await PostForm("/Account/Login", new Dictionary<string, string>
        {
            ["EmailOrUserName"] = email, ["Password"] = password
        });
        var allowed = await Http.GetAsync("/OobeFeedback");
        Assert.AreEqual(HttpStatusCode.OK, allowed.StatusCode);
        Assert.IsTrue(allowed.Headers.CacheControl!.NoStore);
        Assert.Contains("/OobeFeedback", await Http.GetStringAsync("/Dashboard"));
    }

    [TestMethod]
    public async Task EmptyStateAndUntrustedCommentsRenderSafely()
    {
        await LoginAsAdmin();
        var empty = await Http.GetStringAsync("/OobeFeedback");
        Assert.Contains("No ratings match these filters.", empty);
        Assert.DoesNotContain("NaN", empty);
        using (var scope = Server!.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AnduinOSHomeDbContext>();
            db.OobeRatings.Add(new OobeRating { Stars = 2, Comment = "<script>window.feedbackXss=true</script>" });
            await db.SaveChangesAsync();
        }
        var html = await Http.GetStringAsync("/OobeFeedback");
        Assert.Contains("&lt;script&gt;window.feedbackXss=true&lt;/script&gt;", html);
        Assert.DoesNotContain("<script>window.feedbackXss", html);
    }

    [TestMethod]
    public async Task SqliteQueryIncludesWholeUtcDayAndKeepsStatisticsAcrossPages()
    {
        var options = new DbContextOptionsBuilder<SqliteContext>().UseSqlite("DataSource=:memory:").Options;
        await using var db = new SqliteContext(options);
        await db.Database.OpenConnectionAsync();
        await db.Database.MigrateAsync();
        db.OobeRatings.AddRange(Enumerable.Range(0, 26).Select(i => new OobeRating
        {
            Stars = i < 13 ? 1 : 5,
            Comment = i % 2 == 0 ? "Touchpad feedback" : null,
            SubmitTime = new DateTime(2026, 11, 2, 12, 0, i, DateTimeKind.Utc)
        }));
        db.OobeRatings.AddRange(
            new OobeRating { Stars = 3, Comment = "End of day", SubmitTime = new DateTime(2026, 11, 2, 23, 59, 59, DateTimeKind.Utc) },
            new OobeRating { Stars = 2, Comment = "Next day", SubmitTime = new DateTime(2026, 11, 3, 0, 0, 0, DateTimeKind.Utc) });
        await db.SaveChangesAsync();
        using var scope = Server!.Services.CreateScope();
        var controller = new OobeFeedbackController(db)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { RequestServices = scope.ServiceProvider }
            }
        };
        var from = new DateOnly(2026, 11, 2);
        var view = (ViewResult)await controller.Index(from, from, page: 2);
        var model = (IndexViewModel)view.Model!;
        Assert.AreEqual(27L, model.Summary.Count);
        Assert.AreEqual(3d, model.Summary.Average);
        Assert.AreEqual(3d, model.Summary.Median);
        Assert.AreEqual(104d / 27, model.Summary.Variance!.Value, .000001);
        Assert.AreEqual(2, model.Feedback.Count);
        Assert.AreEqual(14L, model.CommentCount);
        Assert.AreEqual(1, model.Trend.Count);

        view = (ViewResult)await controller.Index(from, from, stars: 5, search: "Touchpad", commentsOnly: true, page: 99);
        model = (IndexViewModel)view.Model!;
        Assert.AreEqual(6L, model.Summary.Count);
        Assert.AreEqual(5d, model.Summary.Median);
        Assert.AreEqual(0d, model.Summary.Variance);
        Assert.AreEqual(1, model.Page);
        Assert.IsTrue(model.Feedback.All(x => x.Stars == 5 && x.Comment == "Touchpad feedback"));
    }

    [TestMethod]
    public async Task RejectsMalformedAndUnboundedFilters()
    {
        await LoginAsAdmin();
        foreach (var query in new[]
                 {
                     "from=not-a-date", "from=2026-11-15&to=2026-11-01", "stars=6", "stars=0",
                     "interval=year", "page=-1", "page=1000001", "from=2000-01-01&to=2026-11-15",
                     "search=" + new string('x', 201)
                 })
        {
            Assert.AreEqual(HttpStatusCode.BadRequest, (await Http.GetAsync("/OobeFeedback?" + query)).StatusCode, query);
        }
    }
}
