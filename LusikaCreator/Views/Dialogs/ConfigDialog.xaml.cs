using System.Windows;
using GalaSoft.MvvmLight.Messaging;
using MaterialDesignThemes.Wpf;
using TestApp.ViewModels.Dialogs;
using TestApp.ViewModels.Helpers;

namespace TestApp.Views.Dialogs
{
    public partial class ConfigDialog
    {
        #region ViewModel

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel",
            typeof (ConfigViewModel),
            typeof (ConfigDialog));

        public ConfigViewModel ViewModel
        {
            get { return (ConfigViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        #endregion

        public ConfigDialog()
        {
            InitializeComponent();
            Messenger.Default.Register<NotificationMessage>(this, Close);
        }

        public void Close(NotificationMessage message)
        {
            if (message.Notification == Messages.DoCloseConfigEditor)
                DialogHost.CloseDialogCommand.Execute(true, this);
        }
    }
}