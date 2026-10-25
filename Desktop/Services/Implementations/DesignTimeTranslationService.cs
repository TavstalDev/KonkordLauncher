using System;
using System.Threading;
using System.Threading.Tasks;
using Tavstal.KonkordLauncher.Desktop.Services.Abstractions;
using Tavstal.KonkordLauncher.Desktop.Translation;

namespace Tavstal.KonkordLauncher.Desktop.Services.Implementations;

public class DesignTimeTranslationService : ITranslationService
{
    public Task ChangeLanguageAsync(string language, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public string Translate(string key)
    {
        try
        {
            return TranslationHelper.Translate(key);
        }
        catch (Exception ex)
        {
            return "NOT_FOUND";
        }
    }

    public string Translate(string key, params object[]? args)
    {
        try
        {
           return TranslationHelper.Translate(key, args);
        }
        catch (Exception ex)
        {
            return "NOT_FOUND";
        }
    }
}
