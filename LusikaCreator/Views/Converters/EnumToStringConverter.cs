using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using TestApp.Models.Enums;
using TestApp.Models.Extentions;

namespace TestApp.Views.Converters
{
    public class EnumToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || value == DependencyProperty.UnsetValue) return DependencyProperty.UnsetValue;

            var type = value.GetType();
            if (type == typeof (string)) return value;
            if (type == typeof (DateTime))
            {
                var date = (DateTime) value;
                return date == DateTime.MinValue ? DateTime.Today.ToString("D") : date.ToString("D");
            }

            if (typeof (IEnumerable).IsAssignableFrom(type))
            {
                var collection = (IList) value;
                //var ret = new List<string>();
                //foreach (var item in collection)
                //{
                //    if (item is string) ret.Add((string) item);
                //    else ret.Add(EnumExtensions.GetDescriptionFromValue(item));
                //}
                var ret = (from object item in collection select EnumExtensions.GetDescriptionFromValue(item)).ToList();
                return ret;
            }

            var description = EnumExtensions.GetDescriptionFromValue(value);
            return description;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (parameter != null && parameter != DependencyProperty.UnsetValue)
            {
                targetType = (Type) parameter;
            }

            if (value == null || value == DependencyProperty.UnsetValue) return DependencyProperty.UnsetValue;

            var description = (string) value;

            if (targetType == typeof (ActionOperation))
                return EnumExtensions.GetValueFromDescription<ActionOperation>(description);

            if (targetType == typeof (ActionSelectorOperand))
                return EnumExtensions.GetValueFromDescription<ActionSelectorOperand>(description);

            if (targetType == typeof (ActionInputOperand))
                return EnumExtensions.GetValueFromDescription<ActionInputOperand>(description);

            if (targetType == typeof (ActionVisibilityOperand))
                return EnumExtensions.GetValueFromDescription<ActionVisibilityOperand>(description);

            if (targetType == typeof (VariableType))
                return EnumExtensions.GetValueFromDescription<VariableType>(description);

            return DependencyProperty.UnsetValue;
        }
    }
}