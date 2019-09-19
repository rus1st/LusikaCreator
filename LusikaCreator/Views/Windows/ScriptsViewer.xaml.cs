using System.Windows;
using TestApp.ViewModels.Windows;

namespace TestApp.Views.Windows
{
    public partial class ScriptsViewer 
    {
        #region ViewModel

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel",
            typeof (ScriptsViewerViewModel),
            typeof (ScriptsViewer));

        public ScriptsViewerViewModel ViewModel
        {
            get { return (ScriptsViewerViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        #endregion

        public ScriptsViewer(ScriptsViewerViewModel viewModel)
        {
            ViewModel = viewModel;
            InitializeComponent();
        }
    }
}