using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Platform;
using YamlDotNet.RepresentationModel;

namespace Tavstal.KonkordLauncher.Desktop.Services.Localization;

public class DesignTimeTranslationService : ITranslationService
{
    private static readonly Lazy<Dictionary<string, string>> Translations = new(LoadTranslations);

    public Task ChangeLanguageAsync(string language, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public string Translate(string key)
    {
        return Translations.Value.GetValueOrDefault(key, key);
    }

    public string Translate(string key, params object[]? args)
    {
        var translation = Translate(key);
        if (args is null or { Length: 0 })
            return translation;

        try
        {
            return string.Format(translation, args);
        }
        catch (FormatException)
        {
            return translation;
        }
    }

    private static Dictionary<string, string> LoadTranslations()
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        try
        {
            using var stream = OpenTranslationStream();
            if (stream is null)
                return result;

            using TextReader reader = new StreamReader(stream);
            var yaml = new YamlStream();
            yaml.Load(reader);
            if (yaml.Documents.Count == 0 || yaml.Documents[0].RootNode is not YamlMappingNode root)
                return result;

            Flatten(root, string.Empty, result);
        }
        catch
        {
            // Design-time previews should still render even if the translation resource is unavailable.
        }

        return result;
    }

    private static Stream? OpenTranslationStream()
    {
        try
        {
            return AssetLoader.Open(new Uri("avares://KonkordLauncher/Assets/Lang/en.yml"));
        }
        catch
        {
            return Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("Tavstal.KonkordLauncher.Assets.Lang.en.yml");
        }
    }

    private static void Flatten(YamlMappingNode node, string prefix, Dictionary<string, string> into)
    {
        foreach (var (keyNode, valueNode) in node.Children)
        {
            if (keyNode is not YamlScalarNode keyScalar)
                continue;

            var key = string.IsNullOrEmpty(prefix)
                ? keyScalar.Value ?? string.Empty
                : $"{prefix}.{keyScalar.Value}";

            switch (valueNode)
            {
                case YamlScalarNode scalar:
                    into[key] = scalar.Value ?? string.Empty;
                    break;
                case YamlMappingNode mapping:
                    Flatten(mapping, key, into);
                    break;
                case YamlSequenceNode sequence:
                    for (var i = 0; i < sequence.Children.Count; i++)
                        if (sequence.Children[i] is YamlScalarNode item)
                            into[$"{key}[{i}]"] = item.Value ?? string.Empty;
                    break;
            }
        }
    }
}
