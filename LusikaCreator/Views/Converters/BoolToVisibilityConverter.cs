using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TestApp.Views.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public Visibility TrueValue { get; set; } = Visibility.Visible;

        public Visibility FalseValue { get; set; } = Visibility.Collapsed;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var bValue = false;
            if (value is bool) bValue = (bool) value;

            if (parameter is bool) bValue = !bValue;

            return bValue ? TrueValue : FalseValue;
        }

        public Visibility Convert(bool value)
        {
            return value ? TrueValue : FalseValue;
        }

        public bool ConvertBack(Visibility value)
        {
            return value as Visibility? == TrueValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value as Visibility? == TrueValue;
        }
    }
}