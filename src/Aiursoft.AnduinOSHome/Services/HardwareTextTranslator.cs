using Aiursoft.AnduinOSHome.Configuration;
using Aiursoft.Canon;
using Aiursoft.Dotlang.Shared;
using Aiursoft.GptClient.Services;
using Microsoft.Extensions.Options;

namespace Aiursoft.AnduinOSHome.Services;

public interface IHardwareTextTranslator
{
    Task<string> TranslateAsync(string text, string language);
}

public class HardwareTextTranslator(
    GlobalSettingsService settingsService,
    RetryEngine retryEngine,
    ILogger<OllamaBasedTranslatorEngine> logger,
    ChatClient chatClient,
    MarkdownShredder shredder) : IHardwareTextTranslator
{
    public async Task<string> TranslateAsync(string text, string language)
    {
        if (string.IsNullOrWhiteSpace(text)) return text;
        if (!await settingsService.IsAiLocalizationEnabledAsync())
            throw new InvalidOperationException("Hardware translation is disabled.");
        var engine = new OllamaBasedTranslatorEngine(Options.Create(new TranslateOptions
        {
            OllamaInstance = await settingsService.GetSettingValueAsync(SettingsMap.OpenAiInstance),
            OllamaModel = await settingsService.GetSettingValueAsync(SettingsMap.OpenAiLocalizationModel),
            OllamaToken = await settingsService.GetSettingValueAsync(SettingsMap.OpenAiApiToken)
        }), retryEngine, logger, chatClient, shredder);
        return await engine.TranslateAsync(text, language);
    }
}
