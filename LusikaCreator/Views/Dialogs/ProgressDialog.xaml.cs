using System.Windows;
using System.Windows.Controls;
using GalaSoft.MvvmLight.Messaging;
using MaterialDesignThemes.Wpf;
using TestApp.Models;
using TestApp.ViewModels.Dialogs;
using TestApp.ViewModels.Helpers;

namespace TestApp.Views.Dialogs
{
    public partial class ProgressDialog : UserControl
    {
        #region ViewModel

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel",
            typeof (ProgressDialogViewModel),
            typeof (ProgressDialog));

        public ProgressDialogViewModel ViewModel
        {
            get { return (ProgressDialogViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        #endregion

        public event Handlers.EmptyHandler OnLoaded;

        public ProgressDialog()
        {
            InitializeComponent();
            Messenger.Default.Register<NotificationMessage>(this, Close);
        }

        private void Close(NotificationMessage message)
        {
            if (message.Notification == Messages.DoCloseProgress)
                Close();
        }

        private void Close()
        {
            DialogHost.CloseDialogCommand.Execute(true, this);
        }

        private void ProgressDialog_OnLoaded(object sender, RoutedEventArgs e)
        {
            OnLoaded?.Invoke();
        }
    }
}