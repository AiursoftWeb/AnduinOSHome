using Aiursoft.AnduinOSHome.Authorization;
using Aiursoft.AnduinOSHome.Entities;
using Aiursoft.AnduinOSHome.Models.OobeFeedbackViewModels;
using Aiursoft.AnduinOSHome.Services;
using Aiursoft.UiStack.Navigation;
using Aiursoft.WebTools.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.AnduinOSHome.Controllers;

[Authorize(Policy = AppPermissionNames.ViewOobeFeedbackData)]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
[LimitPerMin]
public class OobeFeedbackController(AnduinOSHomeDbContext db) : Controller
{
    [RenderInNavBar(NavGroupName = "Administration", NavGroupOrder = 9999,
        CascadedLinksGroupName = "OOBE Feedback", CascadedLinksIcon = "message-square-heart",
        CascadedLinksOrder = 30, LinkText = "OOBE Feedback", LinkOrder = 1)]
    [HttpGet]
    public async Task<IActionResult> Index(DateOnly? from = null, DateOnly? to = null,
        string interval = "day", int? stars = null, string? search = null,
        bool commentsOnly = false, int page = 1, CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var start = from ?? today.AddDays(-29);
        var end = to ?? today;
        if (!ModelState.IsValid || start > end || end.DayNumber - start.DayNumber > 3660 ||
            start.Year < 2000 || end.Year > 9998 || page < 1 || page > 1000000 ||
            (stars.HasValue && stars is not (>= 1 and <= 5)) ||
            interval is not ("day" or "week" or "month") || search?.Length > 200)
        {
            return BadRequest();
        }

        var startUtc = start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var endExclusiveUtc = end.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var query = db.OobeRatings.AsNoTracking()
            .Where(x => x.SubmitTime >= startUtc && x.SubmitTime < endExclusiveUtc);
        if (stars.HasValue) query = query.Where(x => x.Stars == stars.Value);
        if (commentsOnly) query = query.Where(x => x.Comment != null && x.Comment.Trim() != "");
        search = search?.Trim();
        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(x => x.Comment != null && x.Comment.Contains(search));
        }

        var daily = await query.GroupBy(x => new { x.SubmitTime.Date, x.Stars })
            .Select(x => new DailyRatingCount(x.Key.Date, x.Key.Stars, x.LongCount()))
            .ToListAsync(cancellationToken);
        var summary = OobeFeedbackStatistics.Summarize(daily);
        var lastPage = Math.Max(1, (summary.Count + IndexViewModel.PageSize - 1) / IndexViewModel.PageSize);
        page = (int)Math.Min(page, lastPage);
        var feedback = await query.OrderByDescending(x => x.SubmitTime).ThenBy(x => x.Id)
            .Skip((page - 1) * IndexViewModel.PageSize).Take(IndexViewModel.PageSize)
            .ToListAsync(cancellationToken);
        return this.StackView(new IndexViewModel
        {
            From = start, To = end, Interval = interval, Stars = stars, Search = search,
            CommentsOnly = commentsOnly, Page = page, Summary = summary, Feedback = feedback,
            CommentCount = await query.LongCountAsync(x => x.Comment != null && x.Comment.Trim() != "", cancellationToken),
            Trend = OobeFeedbackStatistics.BuildTrend(daily, start, end, interval)
        });
    }
}
