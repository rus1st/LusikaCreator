using System;
using System.Globalization;
using System.Windows.Data;

namespace TestApp.Views.Converters
{
    public class FontWeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is string) || ((string) value).Length < 3) return System.Windows.FontWeights.Normal;

            var s = ((string) value).Trim();

            return s == s.ToUpper()
                ? System.Windows.FontWeights.Bold
                : System.Windows.FontWeights.Normal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}