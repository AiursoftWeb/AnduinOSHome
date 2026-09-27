using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Aiursoft.AnduinOSHome.Tests.UnitTests;

[TestClass]
public class LocalizationArchitectureTests
{
    [TestMethod]
    public void RazorViewsDoNotEmbedJavaScript()
    {
        var views = Path.Combine(GetProjectRoot(), "src", "Aiursoft.AnduinOSHome", "Views");
        foreach (var path in Directory.EnumerateFiles(views, "*.cshtml", SearchOption.AllDirectories))
        {
            var source = File.ReadAllText(path);
            Assert.IsFalse(Regex.IsMatch(source, @"<script\b(?![^>]*\bsrc\s*=)[^>]*>", RegexOptions.IgnoreCase), path);
            Assert.IsFalse(Regex.IsMatch(source, @"\son(?:click|change|input|submit)\s*=", RegexOptions.IgnoreCase), path);
        }
    }

    [TestMethod]
    public void BrowserScriptsDoNotContainRazorLocalizationCalls()
    {
        var root = Path.Combine(GetProjectRoot(), "src", "Aiursoft.AnduinOSHome", "wwwroot");
        foreach (var path in Directory.EnumerateFiles(root, "*.js", SearchOption.AllDirectories)
                     .Where(path => !path.Contains("node_modules", StringComparison.Ordinal)))
        {
            var source = File.ReadAllText(path);
            Assert.DoesNotContain("@Localizer[", source, StringComparison.Ordinal);
            Assert.DoesNotContain("@Html.", source, StringComparison.Ordinal);
        }
    }

    [TestMethod]
    public void HardwareCatalogTranslationsCoverEveryPageString()
    {
        var root = Path.Combine(GetProjectRoot(), "src", "Aiursoft.AnduinOSHome");
        var locales = Directory.EnumerateFiles(Path.Combine(root, "Resources", "Services"), "HardwareLabels.*.resx")
            .Select(path => Path.GetFileNameWithoutExtension(path)["HardwareLabels.".Length..]).ToArray();
        Assert.IsTrue(locales.Length >= 27);
        var views = new[]
        {
            "Hardware/Index", "Hardware/Details", "Hardware/_HardwareInsight",
            "ManageHardware/Index", "ManageHardware/Edit", "ManageHardware/Localize",
            "Shared/_HardwareCards", "Shared/Components/RecommendedHardware/Default"
        };
        foreach (var view in views)
        {
            var source = File.ReadAllText(Path.Combine(root, "Views", view + ".cshtml"));
            var keys = Regex.Matches(source, "Localizer\\[\"([^\"]+)\"\\]")
                .Select(match => match.Groups[1].Value).Distinct().ToArray();
            foreach (var locale in locales)
            {
                var path = Path.Combine(root, "Resources", "Views", $"{view}.{locale}.resx");
                Assert.IsTrue(File.Exists(path), path);
                var translated = XDocument.Load(path).Descendants("data")
                    .Select(element => (string?)element.Attribute("name")).ToHashSet();
                var missing = keys.Where(key => !translated.Contains(key)).ToArray();
                Assert.AreEqual(0, missing.Length, $"{path}: missing {string.Join(", ", missing)}");
            }
        }

        var labels = File.ReadAllText(Path.Combine(root, "Services", "HardwareLabels.cs"));
        var labelKeys = Regex.Matches(labels, "localizer\\[\"([^\"]+)\"\\]")
            .Select(match => match.Groups[1].Value).Distinct().ToArray();
        foreach (var locale in locales)
        {
            var path = Path.Combine(root, "Resources", "Services", $"HardwareLabels.{locale}.resx");
            var translated = XDocument.Load(path).Descendants("data")
                .Select(element => (string?)element.Attribute("name")).ToHashSet();
            var missing = labelKeys.Where(key => !translated.Contains(key)).ToArray();
            Assert.AreEqual(0, missing.Length, $"{path}: missing {string.Join(", ", missing)}");
        }

        var controller = File.ReadAllText(Path.Combine(root, "Controllers", "ManageHardwareController.cs"));
        var controllerKeys = Regex.Matches(controller, "localizer\\[\"([^\"]+)\"\\]")
            .Select(match => match.Groups[1].Value).Distinct().ToArray();
        foreach (var locale in locales)
        {
            var path = Path.Combine(root, "Resources", "Controllers", $"ManageHardwareController.{locale}.resx");
            Assert.IsTrue(File.Exists(path), path);
            var translated = XDocument.Load(path).Descendants("data")
                .Select(element => (string?)element.Attribute("name")).ToHashSet();
            var missing = controllerKeys.Where(key => !translated.Contains(key)).ToArray();
            Assert.AreEqual(0, missing.Length, $"{path}: missing {string.Join(", ", missing)}");
        }
    }

    private static string GetProjectRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !Directory.Exists(Path.Combine(directory.FullName, "src", "Aiursoft.AnduinOSHome")))
        {
            directory = directory.Parent;
        }
        return directory?.FullName ?? throw new DirectoryNotFoundException("Project root not found.");
    }
}
