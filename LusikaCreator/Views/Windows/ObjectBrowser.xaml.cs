using System.Windows;
using TestApp.ViewModels;
using TestApp.ViewModels.Windows;

namespace TestApp.Views.Windows
{
    public partial class ObjectBrowser
    {
        #region ViewModel

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel",
            typeof (ObjectBrowserViewModel),
            typeof (ObjectBrowser));

        public ObjectBrowserViewModel ViewModel
        {
            get { return (ObjectBrowserViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        #endregion

        public ObjectBrowser(ObjectBrowserViewModel viewModel)
        {
            ViewModel = viewModel;
            InitializeComponent();
        }
    }
}