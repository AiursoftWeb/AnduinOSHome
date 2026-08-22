using Aiursoft.AnduinOSHome.Configuration;

namespace Aiursoft.AnduinOSHome.Tests.UnitTests;

[TestClass]
public class ReleaseArtifactTests
{
    [TestMethod]
    public void AcceptsCompleteHttpsArtifactSet()
    {
        var artifact = CreateArtifact("https://cf.anduinos.com/AnduinOS-2.0.2-amd64.sha256");

        Assert.IsTrue(artifact.HasValidUrls());
        Assert.AreEqual("AnduinOS-2.0.2-amd64.iso", artifact.IsoFileName);
    }

    [TestMethod]
    public void RejectsCdnRootAsChecksumUrl()
    {
        var artifact = CreateArtifact("https://cf.anduinos.com/");

        Assert.IsFalse(artifact.HasValidUrls());
    }

    private static ReleaseArtifact CreateArtifact(string checksumUrl)
    {
        return new ReleaseArtifact
        {
            IsoUrl = "https://cf.anduinos.com/AnduinOS-2.0.2-amd64.iso",
            TorrentUrl = "https://cf.anduinos.com/AnduinOS-2.0.2-amd64.torrent",
            ChecksumUrl = checksumUrl
        };
    }
}
