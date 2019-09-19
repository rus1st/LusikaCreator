using System.Globalization;
using System.Windows.Controls;

namespace TestApp.Views.Validation
{
    public class NotEmptyValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            const string message = "Не заполнено";

            var isInEditMode = Wrapper?.Properties?.IsInEditMode ?? false;
            if (isInEditMode) return new ValidationResult(true, null);

            var isRequired = Wrapper?.IsRequired ?? false;
            if (!isRequired) return new ValidationResult(true, null);

            if (value == null) return new ValidationResult(false, message);
            if (value is string)
            {
                if (string.IsNullOrEmpty((string) value)) return new ValidationResult(false, message);
            }

            return new ValidationResult(true, null);
        }

        public ValidationWrapper Wrapper { get; set; }
    }
}