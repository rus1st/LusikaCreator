using System.Windows;
using GalaSoft.MvvmLight.Messaging;
using MaterialDesignThemes.Wpf;
using TestApp.ViewModels.Dialogs;
using TestApp.ViewModels.Helpers;

namespace TestApp.Views.Dialogs
{
    public partial class TreeMenu
    {
        #region ViewModel

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel",
            typeof (TreeMenuViewModel),
            typeof (TreeMenu));

        public TreeMenuViewModel ViewModel
        {
            get { return (TreeMenuViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        #endregion

        public TreeMenu(TreeMenuViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
            Messenger.Default.Register<NotificationMessage>(this, Close);
        }

        private void Close(NotificationMessage message)
        {
            if (message.Notification == Messages.DoCloseTreeMenu)
                DialogHost.CloseDialogCommand.Execute(true, this);
        }
    }
}