using System;
using System.Windows;
using System.Windows.Data;

namespace HoradotTV.Converters;

internal class BooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is bool boolean && boolean)
        {
            return Visibility.Visible;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is Visibility visibility && visibility == Visibility.Visible)
        {
            return true;
        }
        return false;
    }
}

internal class BooleanToVisibilityHiddenConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is bool boolean && boolean)
        {
            return Visibility.Visible;
        }
        return Visibility.Hidden;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is Visibility visibility && visibility == Visibility.Visible)
        {
            return true;
        }
        return false;
    }
}

internal class StringToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is string str && str.Length > 0)
        {
            return Visibility.Visible;
        }
        return Visibility.Hidden;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is Visibility visibility && visibility == Visibility.Visible)
        {
            return true;
        }
        return false;
    }
}
