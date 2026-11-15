using Aiursoft.AnduinOSHome.Services;

namespace Aiursoft.AnduinOSHome.Tests.UnitTests;

[TestClass]
public class OobeFeedbackStatisticsTests
{
    [TestMethod]
    public void EmptyRatingsHaveNoInventedScores()
    {
        var summary = OobeFeedbackStatistics.Summarize([]);
        Assert.AreEqual(0L, summary.Count);
        Assert.IsNull(summary.Average);
        Assert.IsNull(summary.Median);
        Assert.IsNull(summary.Variance);
    }

    [TestMethod]
    public void EvenMedianAndPopulationVarianceUseSubmissionWeights()
    {
        var summary = OobeFeedbackStatistics.Summarize([
            new DailyRatingCount(new DateTime(2026, 11, 1), 1, 2),
            new DailyRatingCount(new DateTime(2026, 11, 2), 5, 2)]);
        Assert.AreEqual(4L, summary.Count);
        Assert.AreEqual(3d, summary.Average);
        Assert.AreEqual(3d, summary.Median);
        Assert.AreEqual(4d, summary.Variance);
        CollectionAssert.AreEqual(new long[] { 2, 0, 0, 0, 2 }, summary.Distribution.ToArray());
    }

    [TestMethod]
    public void SingletonHasZeroPopulationVarianceAndOddMedianIsExact()
    {
        var singleton = OobeFeedbackStatistics.Summarize([new DailyRatingCount(DateTime.UtcNow, 4, 1)]);
        Assert.AreEqual(4d, singleton.Median);
        Assert.AreEqual(0d, singleton.Variance);
        var odd = OobeFeedbackStatistics.Summarize([
            new DailyRatingCount(DateTime.UtcNow, 1, 1),
            new DailyRatingCount(DateTime.UtcNow, 4, 2)]);
        Assert.AreEqual(3d, odd.Average);
        Assert.AreEqual(4d, odd.Median);
        Assert.AreEqual(2d, odd.Variance);
    }

    [TestMethod]
    public void WeeklyStatisticsAreWeightedAndEmptyPeriodsRemainGaps()
    {
        var trend = OobeFeedbackStatistics.BuildTrend([
            new DailyRatingCount(new DateTime(2026, 11, 2), 1, 1),
            new DailyRatingCount(new DateTime(2026, 11, 3), 5, 3)],
            new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 15), "week");
        Assert.AreEqual(3, trend.Count);
        Assert.AreEqual(new DateOnly(2026, 10, 26), trend[0].Date);
        Assert.IsNull(trend[0].Summary.Average);
        Assert.AreEqual(4L, trend[1].Summary.Count);
        Assert.AreEqual(4d, trend[1].Summary.Average);
        Assert.AreEqual(5d, trend[1].Summary.Median);
        Assert.AreEqual(3d, trend[1].Summary.Variance);
        Assert.AreEqual(0L, trend[2].Summary.Count);
    }

    [TestMethod]
    public void MonthlyBucketsCrossYearBoundaries()
    {
        var trend = OobeFeedbackStatistics.BuildTrend([
            new DailyRatingCount(new DateTime(2026, 12, 31), 5, 1)],
            new DateOnly(2026, 12, 15), new DateOnly(2027, 1, 2), "month");
        Assert.AreEqual(2, trend.Count);
        Assert.AreEqual(new DateOnly(2027, 1, 1), trend[1].Date);
        Assert.IsNull(trend[1].Summary.Variance);
    }
}
