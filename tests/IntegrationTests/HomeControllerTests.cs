namespace Aiursoft.AnduinOSHome.Tests.IntegrationTests;

[TestClass]
public class HomeControllerTests : TestBase
{
    private const string Amd64ChecksumUrl = "https://cf.anduinos.com/AnduinOS-2.0.2-amd64.sha256";
    private const string Arm64ChecksumUrl = "https://cf.anduinos.com/AnduinOS-2.0.2-arm64.sha256";
    private const string SocialPreviewImageUrl = "https://www.anduinos.com/sc.webp";
    private const string SocialPreviewTitle = "Open Source &amp; Linux";
    private const string SocialPreviewDescription = "AnduinOS is a custom Ubuntu-based Linux distribution that offers a familiar and easy-to-use experience for anyone moving to Linux.";
    private const string ReadyToUseText = "The ISO is just 2.54 GB in size. Like Ubuntu, AnduinOS is simple to install and meets your daily needs without additional configuration or complicated operations.";
    private const string FriendlyInterfaceText = "The GNOME-based desktop environment has a beautiful interface and intuitive human-computer interactions that fit user habits, allowing you to quickly get started with AnduinOS without a steep learning curve.";
    private const string OldReadyToUseText = "The ISO is just 2.54 GB in size. Similar to Ubuntu, it is simple to install and can meet your daily needs without additional configuration or complicated operations.";
    private const string OldFriendlyInterfaceText = "The GNOME-based desktop environment have beautiful interfaces and human-computer interactions that fit user habits, allowing you to quickly get started with AnduinOS without too much learning cost.";

    [TestMethod]
    public async Task GetIndex()
    {
        var url = "/";
        var response = await Http.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();
        Assert.AreEqual(Amd64ChecksumUrl, GetAnchorHrefById(html, "amd64-checksum-link"));
        Assert.AreEqual(Arm64ChecksumUrl, GetAnchorHrefById(html, "arm64-checksum-link"));
        Assert.AreEqual("/Compare.html", GetAnchorHrefById(html, "compare-distributions-link"));
        Assert.Contains("Technical Specifications", html, StringComparison.Ordinal);
        Assert.Contains("System Requirements", html, StringComparison.Ordinal);
        Assert.Contains("Btrfs", html, StringComparison.Ordinal);
        Assert.Contains(ReadyToUseText, html, StringComparison.Ordinal);
        Assert.Contains(FriendlyInterfaceText, html, StringComparison.Ordinal);
        Assert.DoesNotContain(OldReadyToUseText, html, StringComparison.Ordinal);
        Assert.DoesNotContain(OldFriendlyInterfaceText, html, StringComparison.Ordinal);
        Assert.DoesNotContain("data-comparison-root", html, StringComparison.Ordinal);
        Assert.DoesNotContain("css/compare.css", html, StringComparison.Ordinal);
        Assert.Contains($"<meta property=\"og:title\" content=\"{SocialPreviewTitle}\">", html, StringComparison.Ordinal);
        Assert.Contains("<meta property=\"og:description\"", html, StringComparison.Ordinal);
        Assert.Contains($"content=\"{SocialPreviewDescription}\">", html, StringComparison.Ordinal);
        Assert.Contains($"<meta property=\"og:image\" content=\"{SocialPreviewImageUrl}\">", html, StringComparison.Ordinal);
        Assert.Contains("<meta property=\"og:image:alt\" content=\"AnduinOS Main screenshot\">", html, StringComparison.Ordinal);
        Assert.Contains("<meta name=\"twitter:card\" content=\"summary_large_image\">", html, StringComparison.Ordinal);
        Assert.Contains($"<meta name=\"twitter:title\" content=\"{SocialPreviewTitle}\">", html, StringComparison.Ordinal);
        Assert.Contains($"<meta name=\"twitter:image\" content=\"{SocialPreviewImageUrl}\">", html, StringComparison.Ordinal);
        Assert.Contains("<meta name=\"twitter:image:alt\" content=\"AnduinOS Main screenshot\">", html, StringComparison.Ordinal);
    }

    [TestMethod]
    public async Task GetIndexInEnGb()
    {
        var cultureResponse = await Http.GetAsync("/Culture/Set?culture=en-GB&returnUrl=/");
        Assert.AreEqual(System.Net.HttpStatusCode.Found, cultureResponse.StatusCode);

        var response = await Http.GetAsync("/");
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();

        Assert.Contains(ReadyToUseText, html, StringComparison.Ordinal);
        Assert.Contains(FriendlyInterfaceText, html, StringComparison.Ordinal);
        Assert.DoesNotContain(OldReadyToUseText, html, StringComparison.Ordinal);
        Assert.DoesNotContain(OldFriendlyInterfaceText, html, StringComparison.Ordinal);
    }

    [TestMethod]
    public async Task CompareRendersInteractiveDistributionComparisonContract()
    {
        var response = await Http.GetAsync("/Compare.html");
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();

        Assert.Contains("data-comparison-root", html, StringComparison.Ordinal);
        Assert.AreEqual(20, CountOccurrences(html, "data-comparison-row=\""));
        Assert.AreEqual(12, CountOccurrences(html, "comparison-row--extra"));
        Assert.AreEqual(3, CountOccurrences(html, "data-comparison-select=\""));
        Assert.AreEqual(80, CountOccurrences(html, "data-comparison-detail=\""));
        Assert.AreEqual(80, CountOccurrences(html, "<template id=\"comparison-detail-"));
        Assert.Contains("Btrfs default · ext4 optional", html, StringComparison.Ordinal);
        Assert.Contains("data-comparison-expand", html, StringComparison.Ordinal);
        Assert.Contains("data-comparison-dialog", html, StringComparison.Ordinal);
        Assert.Contains("distribution comparison", html, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("css/compare.css", html, StringComparison.Ordinal);
        Assert.Contains("js/compare.js", html, StringComparison.Ordinal);
        Assert.Contains("comparison-status--first", html, StringComparison.Ordinal);
        Assert.Contains("comparison-status--provided", html, StringComparison.Ordinal);
        Assert.Contains("comparison-status--none", html, StringComparison.Ordinal);
        Assert.Contains("data-comparison-mobile-selected-label", html, StringComparison.Ordinal);
        Assert.Contains("Linux Mint 22.3 release announcement", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Linux Mint installation guide", html, StringComparison.Ordinal);
    }

    [TestMethod]
    public async Task GetHistoryBuilds()
    {
        var url = "/HistoryBuilds.html";
        var response = await Http.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();
        Assert.IsTrue(html.Contains("Historical Builds"));
        Assert.IsTrue(html.Contains("historyAccordion"));
        Assert.IsTrue(html.Contains("history.js"));
        Assert.IsTrue(html.Contains("2.0"));
        Assert.IsTrue(html.Contains("Resolute Raccoon"));
        Assert.IsTrue(html.Contains("1.0"));
    }

    [TestMethod]
    [DataRow("amd64", "https://cf.anduinos.com/AnduinOS-2.0.2-amd64.iso", Amd64ChecksumUrl,
        "AnduinOS-2.0.2-amd64.iso")]
    [DataRow("amd64-torrent", "https://cf.anduinos.com/AnduinOS-2.0.2-amd64.torrent", Amd64ChecksumUrl,
        "AnduinOS-2.0.2-amd64.iso")]
    [DataRow("arm64", "https://cf.anduinos.com/AnduinOS-2.0.2-arm64.iso", Arm64ChecksumUrl,
        "AnduinOS-2.0.2-arm64.iso")]
    [DataRow("arm64-torrent", "https://cf.anduinos.com/AnduinOS-2.0.2-arm64.torrent", Arm64ChecksumUrl,
        "AnduinOS-2.0.2-arm64.iso")]
    public async Task GetThankYouWithDownloadParam(
        string download,
        string expectedDownloadUrl,
        string expectedChecksumUrl,
        string expectedIsoFileName)
    {
        var response = await Http.GetAsync($"/thankyou.html?download={download}");
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();

        Assert.IsTrue(html.Contains(expectedDownloadUrl, StringComparison.Ordinal));
        Assert.AreEqual(expectedChecksumUrl, GetAnchorHrefById(html, "official-checksum-link"));
        Assert.IsTrue(html.Contains($"sha256sum ./{expectedIsoFileName}", StringComparison.Ordinal));
        Assert.IsFalse(html.Contains("https://cf.anduinos.com/\"", StringComparison.Ordinal));
    }

    [TestMethod]
    public async Task GetThankYouRejectsMissingOrUnknownDownloadParam()
    {
        var response = await Http.GetAsync("/thankyou.html");
        Assert.AreEqual(System.Net.HttpStatusCode.NotFound, response.StatusCode);

        response = await Http.GetAsync("/thankyou.html?download=unknown");
        Assert.AreEqual(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    private static string GetAnchorHrefById(string html, string id)
    {
        var anchor = System.Text.RegularExpressions.Regex.Match(
            html,
            $@"<a\b[^>]*\bid=""{System.Text.RegularExpressions.Regex.Escape(id)}""[^>]*>",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        Assert.IsTrue(anchor.Success, $"Could not find anchor with id '{id}'.");

        var href = System.Text.RegularExpressions.Regex.Match(
            anchor.Value,
            @"\bhref=""([^""]+)""",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        Assert.IsTrue(href.Success, $"Anchor with id '{id}' does not contain an href attribute.");
        return System.Net.WebUtility.HtmlDecode(href.Groups[1].Value);
    }

    private static int CountOccurrences(string value, string pattern)
    {
        var count = 0;
        var offset = 0;
        while ((offset = value.IndexOf(pattern, offset, StringComparison.Ordinal)) >= 0)
        {
            count++;
            offset += pattern.Length;
        }
        return count;
    }
}
