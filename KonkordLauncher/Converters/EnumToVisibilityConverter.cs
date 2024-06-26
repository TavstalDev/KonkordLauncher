﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Tavstal.KonkordLauncher.Converters
{
    [ValueConversion(typeof(Enum), typeof(bool))]
    public class EnumToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || !(value is Enum))
                return Visibility.Collapsed;

            Enum enumValue = (Enum)value;

            return enumValue.Equals(parameter) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
