using Aiursoft.UiStack.Layout;

namespace Aiursoft.AnduinOSHome.Models.HomeViewModels;

public class HistoryBuildsViewModel : UiStackLayoutViewModel
{
    public HistoryBuildsViewModel()
    {
        PageTitle = "Historical Builds";
    }

    public static IReadOnlyList<VersionInfo> AllVersions => VersionData.All;
    public static IReadOnlyDictionary<string, string> AllLanguages => LanguageData.All;
}

public enum VersionStatus { Active, EOL }

public record VersionInfo(
    string Version,
    string LatestPatch,
    string Codename,
    DateOnly Released,
    DateOnly? EndsSupport,
    VersionStatus Status,
    string LanguageCodes,
    string Kernel,
    string Gnome,
    string UbuntuBase,
    string? SpecialNote,
    bool ProvidesIso = false
);

public static class VersionData
{
    // Language groups: the actual sets of language codes per version in the server tree
    private const string Base20 =
        "ar_SA,de_DE,en_US,es_ES,fr_FR,it_IT,ja_JP,ko_KR,nl_NL,pl_PL,pt_BR,pt_PT,ru_RU,sv_SE,th_TH,tr_TR,vi_VN,zh_CN,zh_HK,zh_TW";

    private const string WithEnGb = Base20 + ",en_GB";
    private const string WithRoRo = WithEnGb + ",ro_RO";

    public static readonly IReadOnlyList<VersionInfo> All = new List<VersionInfo>
    {
        new(
            Version:         "2.0",
            LatestPatch:     "2.0.1",
            Codename:        "Resolute Raccoon",
            Released:        new DateOnly(2026, 6, 30),
            EndsSupport:     new DateOnly(2031, 4, 1),
            Status:          VersionStatus.Active,
            LanguageCodes:   "",
            Kernel:          "7.0",
            Gnome:           "50",
            UbuntuBase:      "26.04",
            SpecialNote:     null
        ),
        new(
            Version:         "1.4",
            LatestPatch:     "1.4.2",
            Codename:        "Questing Quokka",
            Released:        new DateOnly(2025, 10, 15),
            EndsSupport:     new DateOnly(2026, 7, 1),
            Status:          VersionStatus.EOL,
            LanguageCodes:   WithRoRo,

            Kernel:          "6.17",
            Gnome:           "49",
            UbuntuBase:      "25.10",
            SpecialNote:     null
        ),
        new(
            Version:         "1.3",
            LatestPatch:     "1.3.9",
            Codename:        "Plucky Puffin",
            Released:        new DateOnly(2025, 5, 1),
            EndsSupport:     new DateOnly(2026, 1, 1),
            Status:          VersionStatus.EOL,
            LanguageCodes:   WithEnGb,

            Kernel:          "6.11",
            Gnome:           "48",
            UbuntuBase:      "25.04",
            SpecialNote:     null
        ),
        new(
            Version:         "1.2",
            LatestPatch:     "1.2.6",
            Codename:        "Oracular Oriole",
            Released:        new DateOnly(2025, 1, 9),
            EndsSupport:     new DateOnly(2025, 7, 1),
            Status:          VersionStatus.EOL,
            LanguageCodes:   WithEnGb,

            Kernel:          "6.8",
            Gnome:           "47",
            UbuntuBase:      "24.10",
            SpecialNote:     null
        ),
        new(
            Version:         "1.1",
            LatestPatch:     "1.1.12",
            Codename:        "Noble Numbat",
            Released:        new DateOnly(2025, 1, 6),
            EndsSupport:     new DateOnly(2029, 4, 1),
            Status:          VersionStatus.Active,
            LanguageCodes:   WithEnGb,

            Kernel:          "6.14",
            Gnome:           "46",
            UbuntuBase:      "24.04",
            SpecialNote:     null,
            ProvidesIso:     true
        ),
        new(
            Version:         "1.0",
            LatestPatch:     "1.0.7",
            Codename:        "Jammy Jellyfish",
            Released:        new DateOnly(2024, 9, 1),
            EndsSupport:     new DateOnly(2026, 6, 30),
            Status:          VersionStatus.EOL,
            LanguageCodes:   Base20,

            Kernel:          "6.8",
            Gnome:           "46",
            UbuntuBase:      "24.04",
            SpecialNote:     null
        ),
    };
}

public static class LanguageData
{
    public static readonly IReadOnlyDictionary<string, string> All = new Dictionary<string, string>
    {
        { "ar_SA", "العربية" },
        { "de_DE", "Deutsch" },
        { "en_GB", "English (United Kingdom)" },
        { "en_US", "English (United States)" },
        { "es_ES", "Español" },
        { "fr_FR", "Français" },
        { "it_IT", "Italiano" },
        { "ja_JP", "日本語" },
        { "ko_KR", "한국어" },
        { "nl_NL", "Nederlands" },
        { "pl_PL", "Polski" },
        { "pt_BR", "Português (Brasil)" },
        { "pt_PT", "Português" },
        { "ro_RO", "Română" },
        { "ru_RU", "Русский" },
        { "sv_SE", "Svenska" },
        { "th_TH", "ภาษาไทย" },
        { "tr_TR", "Türkçe" },
        { "vi_VN", "Tiếng Việt" },
        { "zh_CN", "中文 (中国大陆)" },
        { "zh_HK", "中文 (香港)" },
        { "zh_TW", "中文 (台灣)" },
    };
}
