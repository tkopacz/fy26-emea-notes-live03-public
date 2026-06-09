using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NoteTaker.Wpf.Converters;

/// <summary>
/// Converts a boolean value to its inverse.
/// Used for enabling/disabling controls based on IsLoading state.
/// </summary>
public class InverseBooleanConverter : IValueConverter
{
    /// <summary>
    /// Converts a boolean to its inverse (true becomes false, false becomes true).
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }

        return value;
    }

    /// <summary>
    /// Converts back from inverted boolean to original boolean.
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }

        return value;
    }
}
