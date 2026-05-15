using System;
using System.Diagnostics;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Tavstal.KonkordLauncher.Desktop.Models.Avalonia.Converters;


/// <summary>
/// Converts a boolean value into an opacity value for UI bindings.
/// </summary>
public class BoolToOpacityConverter : IValueConverter
{
    /// <summary>
    /// Converts a boolean into an opacity value.
    /// </summary>
    /// <param name="value">The source value to convert.</param>
    /// <param name="targetType">The target binding type.</param>
    /// <param name="parameter">Optional converter parameter; not used.</param>
    /// <param name="culture">Culture information; not used.</param>
    /// <returns><c>1.0</c> for <c>true</c> or non-boolean values, <c>0.5</c> for <c>false</c>.</returns>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isEnabled)
            return isEnabled ? 1.0 : 0.5;
        return 1.0;
    }

    /// <summary>
    /// Reverse conversion is not supported.
    /// </summary>
    /// <param name="value">The target value to convert back.</param>
    /// <param name="targetType">The source binding type.</param>
    /// <param name="parameter">Optional converter parameter; not used.</param>
    /// <param name="culture">Culture information; not used.</param>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        Debug.WriteLine("BoolToOpacityConverter does not support ConvertBack.");
        return null;
    }
}