using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Avalonia.Data.Converters;

namespace Tavstal.KonkordLauncher.Desktop.Models.Avalonia.Converters;

/// <summary>
/// Converts file metadata into a FontAwesome-style icon glyph for display in the UI.
/// </summary>
public class FileIconConverter : IMultiValueConverter
{
    /// <summary>
    /// Converts the supplied values into a glyph string representing the file type.
    /// </summary>
    /// <param name="values">Input values from the binding expression.</param>
    /// <param name="targetType">The expected target type of the binding result.</param>
    /// <param name="parameter">Optional converter parameter; not used.</param>
    /// <param name="culture">Culture information; not used.</param>
    /// <returns>A string containing a glyph code for the appropriate icon.</returns>
    public object? Convert(IList<object?> values,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        if (values.Count < 2)
            return "\uf15b";

        bool isDir = values[0] as bool? == true;
        string? path = values[1]?.ToString();

        if (isDir)
            return "\uf07b";

        string ext = Path.GetExtension(path ?? "").ToLowerInvariant();

        return ext switch
        {
            // image
            ".png" or ".jpg" or ".jpeg" => "\uf1c5",
            // sound
            ".mp3" or ".ogg" => "\uf1c7",
            // video
            ".mp4" => "\uf1c8",
            // archive
            ".zip" or ".rar" or ".tar" or ".gz" => "\uf1c6",
            // pdf
            ".pdf" => "\uf1c1",
            // text
            ".txt" or ".json" or ".json5" or ".properties" or ".cfg" or ".yml" or ".yaml" or ".xml" or ".xaml" => "\uf15c",
            // code
            ".cs" or ".java" or ".jar" => "\uf1c9",
            // env
            ".env" => "\ue4f0",
            // scripts
            ".bat" or ".sh" or ".zsh" => "\uf120",
            // executables
            ".exe" or ".appimage" => "\uf013",
            // generic file
            _ => "\uf15b"
        };
    }
}