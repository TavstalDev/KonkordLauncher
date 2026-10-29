using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Platform;
using Microsoft.Extensions.Hosting;
using Tavstal.BeaconSDK.Models.Logging;
using Tavstal.BeaconSDK.Services.Abstractions;
using Tavstal.KonkordLauncher.Desktop.Services.Storage;
using Tavstal.KonkordLauncher.Desktop.Translation;
using YamlDotNet.RepresentationModel;

namespace Tavstal.KonkordLauncher.Desktop.Services.Localization;

/// <inheritdoc cref="ITranslationService" />
public class TranslationService : ITranslationService, IHostedService
{
    public delegate void LanguageChangedHandler(string newLanguage);

    private readonly IHttpService _httpService;
    private readonly ILauncherStore _launcherStore;
    private readonly IBeaconLogger _logger;
    private Dictionary<string, string> _defaultTranslations = [];
    private Dictionary<string, string> _translations = [];

    /// <summary>
    ///     Initializes a new instance of the <see cref="TranslationService" /> class.
    /// </summary>
    /// <param name="logger">Logger used to record translation loading and lookup diagnostics.</param>
    /// <param name="httpService">HTTP service used to download translation files when a local copy is not available.</param>
    /// <param name="launcherStore">
    ///     Launcher store used to resolve the translation directory from the current launcher
    ///     settings.
    /// </param>
    public TranslationService(IBeaconLogger<TranslationService> logger, IHttpService httpService,
        ILauncherStore launcherStore)
    {
        _logger = logger;
        _httpService = httpService;
        _launcherStore = launcherStore;
        TranslationHelper.Current = this;
        LoadDefaultTranslations();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        LoadDefaultTranslations();
        _logger.LogDebug("The service has started successfully.");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Stopping the service...");
        _translations.Clear();
        _logger.LogDebug("The service has stopped successfully.");
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task ChangeLanguageAsync(string language, CancellationToken cancellationToken = default)
    {
        var currentLocalizations = _translations;
        try
        {
            if (language.Length is > 5 or < 2)
                throw new Exception("Invalid language code");

            var settings = await _launcherStore.GetSettingsAsync(cancellationToken: cancellationToken);
            var localePath = Path.Combine(settings.Launcher.TranslationsDirectoryPath, $"{language}.yml");
            if (File.Exists(localePath))
            {
                using TextReader reader = new StreamReader(localePath);
                _translations = LoadTranslations(reader);
                LanguageChanged?.Invoke(language);
                return;
            }

            // Do not attempt to download the default language
            if (language == "en")
            {
                _translations = _defaultTranslations;
                LanguageChanged?.Invoke(language);
                return;
            }

            var downloadUrl =
                $"https://raw.githubusercontent.com/TavstalDev/KonkordLauncher/master/Desktoo/Assets/Lang/{language}.yml";
            var resultYml = await _httpService.GetStringAsync(downloadUrl, null, cancellationToken);
            if (resultYml == null)
            {
                _logger.LogWarning("Failed to fetch translations from the URL.");
                return;
            }

            await File.WriteAllTextAsync(localePath, resultYml, cancellationToken);

            using TextReader loc = new StringReader(resultYml);
            _translations = LoadTranslations(loc);
            LanguageChanged?.Invoke(language);
        }
        catch (Exception ex)
        {
            _translations = currentLocalizations;
            _logger.LogCritical(ex, $"Failed to change language: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public string Translate(string key)
    {
        if (_translations.TryGetValue(key, out var translation) && !string.IsNullOrWhiteSpace(translation))
            return translation;

        if (_defaultTranslations.TryGetValue(key, out translation) && !string.IsNullOrWhiteSpace(translation))
            return translation;

        _logger.LogWarning($"Missing translation key: {key}");
        return key;
    }

    /// <inheritdoc />
    public string Translate(string key, params object[]? args)
    {
        var translation = Translate(key);
        if (args is null or { Length: 0 })
            return translation;

        try
        {
            return string.Format(translation, args);
        }
        catch (FormatException ex)
        {
            _logger.LogWarning(
                $"Failed to format translation '{key}' with {args.Length} argument(s): {ex.Message}");
            return translation;
        }
    }

    public static event LanguageChangedHandler? LanguageChanged;

    private void LoadDefaultTranslations()
    {
        try
        {
            using var stream = OpenDefaultTranslationsStream();
            if (stream is null)
            {
                _logger.LogCritical("Default translation resource not found.");
                return;
            }

            using TextReader reader = new StreamReader(stream);
            _defaultTranslations = LoadTranslations(reader);
            if (_translations.Count == 0)
                _translations = _defaultTranslations;
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, $"Failed to load default translations: {ex.Message}");
        }
    }


    private static Stream? OpenDefaultTranslationsStream()
    {
        return Assembly.GetExecutingAssembly()
            .GetManifestResourceStream("Tavstal.KonkordLauncher.Desktop.Assets.Lang.en.yml");
    }

    private Dictionary<string, string> LoadTranslations(TextReader reader)
    {
        var yaml = new YamlStream();
        yaml.Load(reader);

        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        if (yaml.Documents.Count > 0 &&
            yaml.Documents[0].RootNode is YamlMappingNode root)
            Flatten(root, "", result);
        return result;
    }

    private void Flatten(YamlMappingNode node, string prefix, Dictionary<string, string> into)
    {
        foreach (var (k, v) in node.Children)
        {
            var key = prefix.Length == 0 ? ((YamlScalarNode)k).Value! : $"{prefix}.{((YamlScalarNode)k).Value}";
            switch (v)
            {
                case YamlScalarNode s:
                    into[key] = s.Value ?? "";
                    break;
                case YamlMappingNode m:
                    Flatten(m, key, into);
                    break;
                case YamlSequenceNode seq:
                    var i = 0;
                    foreach (var item in seq.Children)
                        if (item is YamlScalarNode si)
                            into[$"{key}[{i++}]"] = si.Value ?? "";
                    break;
            }
        }
    }
}
