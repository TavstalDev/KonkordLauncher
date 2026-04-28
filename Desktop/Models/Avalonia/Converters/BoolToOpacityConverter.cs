using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Tavstal.KonkordLauncher.Desktop.Models.Avalonia.Converters;

public class BoolToOpacityConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isEnabled)
            return isEnabled ? 1.0 : 0.5;
        return 1.0;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}