using System.Windows;
using System.Windows.Controls;
using TestApp.ViewModels.Dialogs;

namespace TestApp.Views.Dialogs
{
    public partial class InputDialog : UserControl
    {
        #region ViewModel

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel",
            typeof (InputDialogViewModel),
            typeof (InputDialog));

        public InputDialogViewModel ViewModel
        {
            get { return (InputDialogViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        #endregion

        public InputDialog()
        {
            InitializeComponent();
        }
    }
}