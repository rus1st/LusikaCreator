using System;
using System.Globalization;
using System.Windows.Data;

namespace TestApp.ViewModels.Helpers
{
    public class MultiValueConverterAdapter : IMultiValueConverter
    {
        public IValueConverter Converter { get; set; }

        private object _lastParameter;

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (Converter == null) return values[0]; // Required for VS design-time
            if (values.Length > 1) _lastParameter = values[1];
            return Converter.Convert(values[0], targetType, _lastParameter, culture);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            if (Converter == null) return new[] {value}; // Required for VS design-time

            return new[] {Converter.ConvertBack(value, targetTypes[0], _lastParameter, culture)};
        }
    }
}