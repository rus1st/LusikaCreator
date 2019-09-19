using System.Windows;
using System.Windows.Controls;
using GalaSoft.MvvmLight.Messaging;
using MaterialDesignThemes.Wpf;
using TestApp.ViewModels.Dialogs;
using TestApp.ViewModels.Helpers;

namespace TestApp.Views.Dialogs
{
    public partial class ActionEditor : UserControl
    {
        #region ViewModel

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel",
            typeof (ActionEditorViewModel),
            typeof (ActionEditor));

        public ActionEditorViewModel ViewModel
        {
            get { return (ActionEditorViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        #endregion

        public ActionEditor()
        {
            InitializeComponent();
            Messenger.Default.Register<NotificationMessage>(this, Close);
        }

        public void Close(NotificationMessage message)
        {
            if (message.Notification == Messages.DoCloseActionEditor)
                DialogHost.CloseDialogCommand.Execute(true, this);
        }
    }
}