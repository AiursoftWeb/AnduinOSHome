using System.Text.RegularExpressions;

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
