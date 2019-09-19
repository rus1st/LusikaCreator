using System;
using System.Windows;
using System.Windows.Data;

namespace TestApp.Views.Converters
{
    public class EmptyNameConverter : IValueConverter
    {
        private const string EmptyName = "<Имя не задано>";

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(value is string) || value == DependencyProperty.UnsetValue) return value;

            return string.IsNullOrEmpty((string) value) ? EmptyName : value;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (!(value is string) || value == DependencyProperty.UnsetValue) return value;

            return (string) value == EmptyName ? string.Empty : value;
        }
    }
}