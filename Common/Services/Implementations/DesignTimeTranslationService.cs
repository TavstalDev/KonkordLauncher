using Tavstal.KonkordLauncher.Common.Services.Abstractions;
using Tavstal.KonkordLauncher.Common.Translation;

namespace Tavstal.KonkordLauncher.Common.Services.Implementations;

public class DesignTimeTranslationService : ITranslationService
{
    public async Task ChangeLanguageAsync(string language, CancellationToken cancellationToken = default) { }

    public string Translate(string key)
    {
        return DefaultTranslationProvider.Translations.GetValueOrDefault(key, "NOT_FOUND");
    }

    public string Translate(string key, params object[]? args)
    {
        var template = DefaultTranslationProvider.Translations.GetValueOrDefault(key, "NOT_FOUND");
        if (args == null || args.Length == 0)
            return template;
        return string.Format(template, args);
    }
}