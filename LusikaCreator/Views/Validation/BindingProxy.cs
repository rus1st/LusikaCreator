using System.Windows;
using TestApp.ViewModels.ObjectProperties;

namespace TestApp.Views.Validation
{
    public class BindingProxy : Freezable
    {
        protected override Freezable CreateInstanceCore()
        {
            return new BindingProxy();
        }

        public ObjectBaseProperties Properties
        {
            get { return (ObjectBaseProperties) GetValue(BasePropertiesProperty); }
            set { SetValue(BasePropertiesProperty, value); }
        }

        public static readonly DependencyProperty BasePropertiesProperty =
            DependencyProperty.Register("Properties", typeof (ObjectBaseProperties), typeof (BindingProxy),
                new PropertyMetadata(null));

        public bool IsRequired
        {
            get { return (bool) GetValue(IsRequiredProperty); }
            set { SetValue(IsRequiredProperty, value); }
        }

        public static readonly DependencyProperty IsRequiredProperty =
            DependencyProperty.Register("IsRequired", typeof (bool), typeof (BindingProxy),
                new PropertyMetadata(null));
    }
}