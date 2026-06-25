using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NoteTaker.Wpf.Converters;

/// <summary>
/// Converts a string to Visibility based on whether it's null, empty, or whitespace.
/// Non-empty strings become Visible, empty/null strings become Collapsed.
/// </summary>
public class StringToVisibilityConverter : IValueConverter
{
    /// <summary>
    /// Converts a string to Visibility (non-empty = Visible, empty = Collapsed).
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string stringValue)
        {
            return string.IsNullOrWhiteSpace(stringValue) ? Visibility.Collapsed : Visibility.Visible;
        }

        return Visibility.Collapsed;
    }

    /// <summary>
    /// Not implemented for one-way binding scenarios.
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
