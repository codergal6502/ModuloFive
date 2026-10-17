using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using Avalonia.Data.Converters;
using Microsoft.OpenApi;

namespace ModuloFivePadClient.DesktopClient.ValueConverters
{
    public class EnumDisplayConverter<T> : IValueConverter where T : struct, Enum
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is T enumValue)
            {
                return enumValue.GetDisplayName();
            }
            else if (value is IEnumerable arr && arr.OfType<object>().All(o => typeof(T).IsAssignableFrom(o.GetType())))
            {
                return arr.OfType<T>().Select(e => e.GetDisplayName());
            }
            else
            {
                return value;
            }
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if ((targetType == typeof(T) || targetType == typeof(T?)) && value is string s)
            {
                var allEnumValues = Enum.GetValues<T>();
                if (Enum.TryParse<T>(s, out T e)) return e;
                else return allEnumValues.Where(e => e.GetDisplayName() == s).FirstOrDefault();

            }
            else
            {
                return null;
            }
        }
    }
    
    public class EnumDisplayConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is Enum enumValue)
            {
                return enumValue.GetDisplayName();
            }
            else if (value is IEnumerable arr && arr.OfType<object>().All(o => typeof(Enum).IsAssignableFrom(o.GetType())))
            {
                return arr.OfType<Enum>().Select(e => e.GetDisplayName());
            }
            else
            {
                return value;
            }
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new InvalidOperationException($"Converting display names while supporting AOT requires using the generic type {nameof(EnumDisplayConverter)}<T>.");
        }
    }

    public class YesNoBooleanDisplayConverter : IValueConverter
    {
        public static bool[] YesNoOptionBooleans => [true, false];
        public static bool[] NoYesOptionBooleans => [false, true];
        public static string[] YesNoOptionStrings => [YES, NO];
        public static string[] NoYesOptionStrings => [NO, YES];
        public static readonly string YES = "Yes";
        public static readonly string NO = "No";

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool b)
            {
                return b ? YES : NO;
            }
            else if (value is IEnumerable e)
            {
                return e.OfType<bool>().Select(b => b ? YES : NO);
            }
            return null;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not string s)
            {
                return null;
            }
            else
            {
                var lower = s.Trim().ToLower();
                if (YES.ToLower() == lower) return true;
                if (NO.ToLower() == lower) return false;
                return null;
            }
        }
    }
}