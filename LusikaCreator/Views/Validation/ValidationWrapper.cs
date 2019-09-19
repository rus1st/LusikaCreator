using System.Windows;
using TestApp.ViewModels.ObjectProperties;

namespace TestApp.Views.Validation
{
    public class ValidationWrapper : DependencyObject
    {
        public static readonly DependencyProperty BasePropertiesProperty =
            DependencyProperty.Register("Properties", typeof (ObjectBaseProperties),
                typeof (ValidationWrapper), new FrameworkPropertyMetadata(null));

        public ObjectBaseProperties Properties
        {
            get { return (ObjectBaseProperties) GetValue(BasePropertiesProperty); }
            set { SetValue(BasePropertiesProperty, value); }
        }

        public static readonly DependencyProperty IsRequiredProperty =
            DependencyProperty.Register("IsRequired", typeof (bool),
                typeof (ValidationWrapper), new FrameworkPropertyMetadata(null));

        public bool IsRequired
        {
            get { return (bool) GetValue(IsRequiredProperty); }
            set { SetValue(IsRequiredProperty, value); }
        }
    }
}