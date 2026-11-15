using Aiursoft.AnduinOSHome.Entities;
using Aiursoft.AnduinOSHome.Services;
using Aiursoft.UiStack.Layout;

namespace Aiursoft.AnduinOSHome.Models.OobeFeedbackViewModels;

public class IndexViewModel : UiStackLayoutViewModel
{
    public IndexViewModel()
    {
        PageTitle = "OOBE Feedback";
    }

    public DateOnly From { get; init; }
    public DateOnly To { get; init; }
    public string Interval { get; init; } = "day";
    public int? Stars { get; init; }
    public string? Search { get; init; }
    public bool CommentsOnly { get; init; }
    public int Page { get; init; }
    public const int PageSize = 25;
    public long CommentCount { get; init; }
    public required RatingSummary Summary { get; init; }
    public required IReadOnlyList<RatingBucket> Trend { get; init; }
    public required IReadOnlyList<OobeRating> Feedback { get; init; }
    public long TotalPages => Math.Max(1, (Summary.Count + PageSize - 1) / PageSize);
}
