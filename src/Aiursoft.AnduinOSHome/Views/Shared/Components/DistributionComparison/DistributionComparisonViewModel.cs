namespace Aiursoft.AnduinOSHome.Views.Shared.Components.DistributionComparison;

public enum ComparisonLevel
{
    FirstClass,
    DefaultProvided,
    Supported,
    Experimental,
    NotDocumented,
    Unavailable
}

public sealed record ComparisonSource(string Label, string Url);

public sealed record ComparisonCell(
    ComparisonLevel Level,
    string Summary,
    string Detail,
    IReadOnlyList<ComparisonSource> Sources);

public sealed record ComparisonItem(
    string Id,
    string Icon,
    string Title,
    string Subtitle,
    bool IsCore,
    ComparisonCell AnduinOs,
    ComparisonCell Zorin,
    ComparisonCell Mint,
    ComparisonCell Ubuntu);

public sealed class DistributionComparisonViewModel
{
    public required IReadOnlyList<ComparisonItem> Items { get; init; }
}
