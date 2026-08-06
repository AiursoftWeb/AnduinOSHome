namespace Aiursoft.AnduinOSHome.Tests.IntegrationTests;

[TestClass]
public class HomeControllerTests : TestBase
{
    private const string Amd64ChecksumUrl = "https://cf.anduinos.com/AnduinOS-2.0.1-amd64.sha256";
    private const string Arm64ChecksumUrl = "https://cf.anduinos.com/AnduinOS-2.0.1-arm64.sha256";

    [TestMethod]
    public async Task GetIndex()
    {
        var url = "/";
        var response = await Http.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();
        Assert.AreEqual(Amd64ChecksumUrl, GetAnchorHrefById(html, "amd64-checksum-link"));
        Assert.AreEqual(Arm64ChecksumUrl, GetAnchorHrefById(html, "arm64-checksum-link"));
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
    [DataRow("amd64", "https://cf.anduinos.com/AnduinOS-2.0.1-amd64.iso", Amd64ChecksumUrl,
        "AnduinOS-2.0.1-amd64.iso")]
    [DataRow("amd64-torrent", "https://cf.anduinos.com/AnduinOS-2.0.1-amd64.torrent", Amd64ChecksumUrl,
        "AnduinOS-2.0.1-amd64.iso")]
    [DataRow("arm64", "https://cf.anduinos.com/AnduinOS-2.0.1-arm64.iso", Arm64ChecksumUrl,
        "AnduinOS-2.0.1-arm64.iso")]
    [DataRow("arm64-torrent", "https://cf.anduinos.com/AnduinOS-2.0.1-arm64.torrent", Arm64ChecksumUrl,
        "AnduinOS-2.0.1-arm64.iso")]
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
}
