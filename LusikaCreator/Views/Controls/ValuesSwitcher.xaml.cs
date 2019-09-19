using System.Windows;
using TestApp.ViewModels.Helpers;

namespace TestApp.Views.Controls
{
    public partial class ValuesSwitcher
    {
        #region ViewModel

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel",
            typeof(ValuesSwitcherViewModel),
            typeof(ValuesSwitcher));

        public ValuesSwitcherViewModel ViewModel
        {
            get { return (ValuesSwitcherViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        #endregion

        public ValuesSwitcher()
        {
            InitializeComponent();
        }
    }
}