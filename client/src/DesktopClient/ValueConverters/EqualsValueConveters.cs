using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace ModuloFivePadClient.DesktopClient.ValueConverters
{
    public class EqualsValueConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value == parameter;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class IsEmptyOrWhitespace : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            bool invert = true;
            if (null == parameter)
            {
                invert = false;
            }
            else if (parameter is string s)
            {
                invert = s.Trim() != string.Empty;
            }
            if (null == value) return !invert;
            return invert ^ (String.Format("{0}", value).Trim() == String.Empty);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}