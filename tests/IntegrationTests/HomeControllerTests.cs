namespace Aiursoft.AnduinOSHome.Tests.IntegrationTests;

[TestClass]
public class HomeControllerTests : TestBase
{
    [TestMethod]
    public async Task GetIndex()
    {
        var url = "/";
        var response = await Http.GetAsync(url);
        response.EnsureSuccessStatusCode();
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
    public async Task GetThankYouWithDownloadParam()
    {
        var response = await Http.GetAsync("/thankyou.html?download=amd64");
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();
        Assert.IsTrue(html.Contains("https://cf.anduinos.com/AnduinOS-2.0.1-amd64.iso"));
        
        response = await Http.GetAsync("/thankyou.html?download=arm64-torrent");
        response.EnsureSuccessStatusCode();
        html = await response.Content.ReadAsStringAsync();
        Assert.IsTrue(html.Contains("https://cf.anduinos.com/AnduinOS-2.0.1-arm64.torrent"));

        response = await Http.GetAsync("/thankyou.html");
        Assert.AreEqual(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }
}
