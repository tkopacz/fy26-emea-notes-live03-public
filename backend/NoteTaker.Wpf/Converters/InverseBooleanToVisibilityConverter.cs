using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NoteTaker.Wpf.Converters;

/// <summary>
/// Converts a boolean value to its inverse Visibility state.
/// True becomes Collapsed, False becomes Visible.
/// </summary>
public class InverseBooleanToVisibilityConverter : IValueConverter
{
    /// <summary>
    /// Converts a boolean to inverse Visibility (true = Collapsed, false = Visible).
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? Visibility.Collapsed : Visibility.Visible;
        }

        return Visibility.Visible;
    }

    /// <summary>
    /// Converts back from Visibility to inverted boolean.
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Visibility visibility)
        {
            return visibility != Visibility.Visible;
        }

        return false;
    }
}
