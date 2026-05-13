using Aiursoft.AnduinOSHome.Models.SystemViewModels;

namespace Aiursoft.AnduinOSHome.Tests;

[TestClass]
public class MigrationEntryTests
{
    [TestMethod]
    public void NameParsedCorrectlyFromStandardId()
    {
        var entry = new MigrationEntry { Id = "20260108110700_AddGlobalSettings" };
        Assert.AreEqual("AddGlobalSettings", entry.Name);
    }

    [TestMethod]
    public void NameReturnsFullIdWhenShorterThan15Chars()
    {
        var entry = new MigrationEntry { Id = "ShortId" };
        Assert.AreEqual("ShortId", entry.Name);
    }

    [TestMethod]
    public void AppliedAtParsedCorrectlyFromStandardId()
    {
        var entry = new MigrationEntry { Id = "20260108110700_AddGlobalSettings" };
        var expected = new DateTime(2026, 1, 8, 11, 7, 0, DateTimeKind.Utc);
        Assert.AreEqual(expected, entry.AppliedAt);
    }

    [TestMethod]
    public void AppliedAtReturnsNullWhenTimestampIsInvalid()
    {
        var entry = new MigrationEntry { Id = "NotATimestamp_SomeMigration" };
        Assert.IsNull(entry.AppliedAt);
    }
}
