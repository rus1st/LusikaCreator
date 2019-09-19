using System.Windows;
using System.Windows.Controls;
using GalaSoft.MvvmLight.Messaging;
using MaterialDesignThemes.Wpf;
using TestApp.ViewModels.Dialogs;
using TestApp.ViewModels.Helpers;

namespace TestApp.Views.Dialogs
{
    public partial class MessageDialog : UserControl
    {
        #region ViewModel

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel",
            typeof(MessageDialogViewModel),
            typeof(MessageDialog));

        public MessageDialogViewModel ViewModel
        {
            get { return (MessageDialogViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        #endregion

        public MessageDialog()
        {
            InitializeComponent();
            Messenger.Default.Register<NotificationMessage>(this, Close);
        }

        private void Close(NotificationMessage message)
        {
            if (message.Notification == Messages.DoCloseMessageDialog)
                DialogHost.CloseDialogCommand.Execute(true, this);
        }
    }
}