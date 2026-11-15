namespace Aiursoft.AnduinOSHome.Services;

public record RatingSummary(long Count, double? Average, double? Variance, double? Median,
    IReadOnlyList<long> Distribution);

public record DailyRatingCount(DateTime Date, int Stars, long Count);

public record RatingBucket(DateOnly Date, RatingSummary Summary);

public static class OobeFeedbackStatistics
{
    // A five-bin histogram gives the exact median without loading individual responses.
    public static RatingSummary Summarize(IEnumerable<DailyRatingCount> counts)
    {
        var histogram = new long[5];
        foreach (var item in counts)
        {
            histogram[item.Stars - 1] += item.Count;
        }
        var total = histogram.Sum();
        if (total == 0) return new RatingSummary(0, null, null, null, histogram);
        var mean = histogram.Select((count, i) => count * (i + 1d)).Sum() / total;
        var variance = histogram.Select((count, i) => count * Math.Pow(i + 1 - mean, 2)).Sum() / total;
        var lowerRank = (total + 1) / 2;
        var upperRank = total / 2 + 1;
        long cumulative = 0;
        var lower = 0;
        var upper = 0;
        for (var i = 0; i < histogram.Length; i++)
        {
            cumulative += histogram[i];
            if (lower == 0 && cumulative >= lowerRank) lower = i + 1;
            if (upper == 0 && cumulative >= upperRank) upper = i + 1;
        }
        return new RatingSummary(total, mean, variance, (lower + upper) / 2d, histogram);
    }

    public static IReadOnlyList<RatingBucket> BuildTrend(IReadOnlyList<DailyRatingCount> counts,
        DateOnly from, DateOnly to, string interval)
    {
        var grouped = counts.GroupBy(x => BucketStart(DateOnly.FromDateTime(x.Date), interval))
            .ToDictionary(x => x.Key, x => Summarize(x));
        var result = new List<RatingBucket>();
        for (var date = BucketStart(from, interval); date <= to; date = NextBucket(date, interval))
        {
            result.Add(new RatingBucket(date, grouped.GetValueOrDefault(date) ?? Summarize([])));
        }
        return result;
    }

    private static DateOnly BucketStart(DateOnly date, string interval) => interval switch
    {
        "week" => date.AddDays(-((int)date.DayOfWeek + 6) % 7),
        "month" => new DateOnly(date.Year, date.Month, 1),
        _ => date
    };

    private static DateOnly NextBucket(DateOnly date, string interval) => interval switch
    {
        "week" => date.AddDays(7),
        "month" => date.AddMonths(1),
        _ => date.AddDays(1)
    };
}
