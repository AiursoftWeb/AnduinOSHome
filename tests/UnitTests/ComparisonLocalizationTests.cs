using System.Globalization;
using System.Resources;
using Aiursoft.AnduinOSHome.Views.Shared.Components.DistributionComparison;
using Microsoft.AspNetCore.Mvc.ViewComponents;

namespace Aiursoft.AnduinOSHome.Tests.UnitTests;

[TestClass]
public class ComparisonLocalizationTests
{
    [TestMethod]
    public void EveryLanguageContainsTheFullComparisonWithoutResourceFallback()
    {
        string[] cultures =
        [
            "ar-SA", "da-DK", "de-DE", "el-GR", "en-GB", "es-ES", "fi-FI", "fr-FR",
            "hi-IN", "id-ID", "it-IT", "ja-JP", "ko-KR", "nl-NL", "pl-PL", "pt-BR",
            "pt-PT", "ro-RO", "ru-RU", "sv-SE", "th-TH", "tr-TR", "uk-UA", "vi-VN",
            "zh-CN", "zh-HK", "zh-TW"
        ];
        var assembly = typeof(DistributionComparison).Assembly;
        var resourceName = assembly.GetManifestResourceNames()
            .Single(name => name.EndsWith(".DistributionComparison.Default.resources", StringComparison.Ordinal));
        var baseName = resourceName[..^".resources".Length];
        var result = (ViewViewComponentResult)new DistributionComparison().Invoke();
        var items = ((DistributionComparisonViewModel)result.ViewData!.Model!).Items;
        var dynamicKeys = items.SelectMany(item => new[] { item.Title, item.Subtitle }
                .Concat(new[] { item.AnduinOs, item.Zorin, item.Mint, item.Ubuntu }
                    .SelectMany(cell => new[] { cell.Summary, cell.Detail }
                        .Concat(cell.Sources.Select(source => source.Label)))))
            .Distinct().ToArray();

        var neutralManager = new ResourceManager(baseName, assembly);
        var neutral = neutralManager.GetResourceSet(CultureInfo.InvariantCulture, true, false)!;
        Assert.IsNotNull(neutral);
        var allKeys = neutral.Cast<System.Collections.DictionaryEntry>()
            .Select(entry => (string)entry.Key).ToArray();
        foreach (var key in dynamicKeys)
        {
            Assert.IsNotNull(neutral.GetString(key), $"Default English resource missing: {key}");
        }

        foreach (var culture in cultures)
        {
            // A fresh manager and tryParents:false prevent cached fallback resources from hiding gaps.
            var manager = new ResourceManager(baseName, assembly);
            var translations = manager.GetResourceSet(CultureInfo.GetCultureInfo(culture), true, false);
            Assert.IsNotNull(translations, $"Missing satellite resources for {culture}");
            foreach (var key in allKeys)
            {
                var value = translations.GetString(key);
                Assert.IsFalse(string.IsNullOrWhiteSpace(value), $"{culture} is missing: {key}");
                if (key.Contains("{0}", StringComparison.Ordinal))
                {
                    Assert.Contains("{0}", value, StringComparison.Ordinal, $"{culture} lost its count placeholder");
                }
                if (key.Contains("Swap", StringComparison.Ordinal))
                {
                    Assert.Contains("Swap", value, StringComparison.Ordinal, $"{culture} translated the Swap term: {key}");
                }
            }
            if (culture != "en-GB")
            {
                Assert.AreNotEqual("View details", translations.GetString("View details"), culture);
                Assert.AreNotEqual(items[0].AnduinOs.Detail, translations.GetString(items[0].AnduinOs.Detail), culture);
            }
            manager.ReleaseAllResources();
        }
        neutralManager.ReleaseAllResources();
    }
}
