using System.Windows;
using TestApp.ViewModels.Windows;

namespace TestApp.Views.Windows
{
    public partial class ToolsPanel
    {
        #region ViewModel

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel",
            typeof (ToolsPanelViewModel),
            typeof (ToolsPanel));

        public ToolsPanelViewModel ViewModel
        {
            get { return (ToolsPanelViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        #endregion

        public ToolsPanel(ToolsPanelViewModel viewModel)
        {
            ViewModel = viewModel;
            InitializeComponent();
        }
    }
}