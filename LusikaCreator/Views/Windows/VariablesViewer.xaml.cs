using System.Windows;
using TestApp.ViewModels.Windows;

namespace TestApp.Views.Windows
{
    public partial class VariablesViewer
    {

        #region ViewModel

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel",
            typeof (VariablesViewerViewModel),
            typeof (VariablesViewer));

        public VariablesViewerViewModel ViewModel
        {
            get { return (VariablesViewerViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        #endregion

        public VariablesViewer(VariablesViewerViewModel viewModel)
        {
            ViewModel = viewModel;
            InitializeComponent();
        }
    }
}