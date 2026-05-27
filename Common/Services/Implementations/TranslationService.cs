using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Tavstal.KonkordLauncher.Common.Helpers;
using Tavstal.KonkordLauncher.Common.Services.Abstractions;
using Tavstal.KonkordLauncher.Common.Translation;
using Tavstal.KonkordLauncher.Core.Helpers.Serialization;
using Tavstal.KonkordLauncher.Core.Services.Abstractions;

namespace Tavstal.KonkordLauncher.Common.Services.Implementations;

/// <inheritdoc cref="ITranslationService"/>
public class TranslationService : IHostedService, ITranslationService
{
    private readonly ILogger _logger;
    private readonly IHttpService _httpService;
    private readonly ILauncherStore _launcherStore;
    private Dictionary<string, string> _translations = new();
    
    public delegate void LanguageChangedHandler(string newLanguage);
    public static event LanguageChangedHandler? LanguageChanged;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="TranslationService"/> class.
    /// </summary>
    /// <param name="logger">Logger used to record translation loading and lookup diagnostics.</param>
    /// <param name="httpService">HTTP service used to download translation files when a local copy is not available.</param>
    /// <param name="launcherStore">Launcher store used to resolve the translation directory from the current launcher settings.</param>
    public TranslationService(ILogger<TranslationService> logger, IHttpService httpService, ILauncherStore launcherStore)
    {
        _logger = logger;
        _httpService = httpService;
        _launcherStore = launcherStore;
        TranslationHelper.Current = this;
    }
    
    /// <inheritdoc/>
    public async Task StartAsync(CancellationToken cancellationToken) => await ChangeLanguageAsync("en", cancellationToken);

    /// <inheritdoc/>
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    /// <inheritdoc/>
    public async Task ChangeLanguageAsync(string language, CancellationToken cancellationToken = default)
    {
        try
        {
            if (language.Length is > 5 or < 2)
                throw new Exception("Invalid language code");
            
            var settings = await _launcherStore.GetSettingsAsync(cancellationToken: cancellationToken);
            string localePath = Path.Combine(settings.Launcher.TranslationsDirectoryPath, $"{language}.json");
            if (File.Exists(localePath))
            {
                Dictionary<string, string>? translations = await JsonHelper.ReadJsonFileAsync<Dictionary<string, string>>(localePath);
                if (translations is null)
                {
                    _logger.LogError("Failed to read local translation file.");
                    return;
                }

                _translations = DefaultTranslationProvider.Translations;
                foreach (var pair in translations)
                {
                    if (string.IsNullOrWhiteSpace(pair.Value) || !_translations.ContainsKey(pair.Key))
                        continue;
                    _translations[pair.Key] = pair.Value;
                }
                LanguageChanged?.Invoke(language);
                return;
            }
            
            // Do not attempt to download the default language
            if (language == "en")
            {
                _translations = DefaultTranslationProvider.Translations;
                await JsonHelper.WriteJsonFileAsync(localePath, _translations, cancellationToken);
                LanguageChanged?.Invoke(language);
                return;
            }
            
            string downloadUrl =
                $"https://raw.githubusercontent.com/TavstalDev/KonkordLauncher/master/docs/translations/{language}.json";

            string? resultJson = await _httpService.GetStringAsync(downloadUrl, cancellationToken);
            if (resultJson == null)
            {
                _logger.LogWarning("Failed to fetch translations from the URL.");
                return;
            }

            Dictionary<string, string>? translation = JsonConvert.DeserializeObject<Dictionary<string, string>>(resultJson);
            if (translation == null)
            {
                _logger.LogError("Failed to deserialize translations from the URL.");
                return;
            }
            
            _translations = DefaultTranslationProvider.Translations;
            foreach (var pair in translation)
            {
                if (string.IsNullOrWhiteSpace(pair.Value) || !_translations.ContainsKey(pair.Key))
                    continue;
                _translations[pair.Key] = pair.Value;
            }
            await JsonHelper.WriteJsonFileAsync(localePath, translation, cancellationToken);
            LanguageChanged?.Invoke(language);
        }
        catch (Exception ex)
        {
            _logger.LogCritical($"Failed to change language: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public string Translate(string key)
    {
        if (!_translations.TryGetValue(key, out var translation) || string.IsNullOrWhiteSpace(translation))
            return DefaultTranslationProvider.Translations[key];
        return translation;
    }

    /// <inheritdoc/>
    public string Translate(string key, params object[]? args)
    {
       string translation = Translate(key);
       if (args is null or { Length: 0 })
           return translation;
       return string.Format(translation, args);
    }
}