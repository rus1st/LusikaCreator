using System.Windows;
using GalaSoft.MvvmLight.Messaging;
using MaterialDesignThemes.Wpf;
using TestApp.ViewModels.Dialogs;
using TestApp.ViewModels.Helpers;

namespace TestApp.Views.Dialogs
{
    public partial class VariableEditor
    {
        #region ViewModel

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel",
            typeof (VariableEditorViewModel),
            typeof (VariableEditor));

        public VariableEditorViewModel ViewModel
        {
            get { return (VariableEditorViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        #endregion

        public VariableEditor()
        {
            InitializeComponent();

            Messenger.Default.Register<NotificationMessage>(this, Close);
        }

        private void Close(NotificationMessage message)
        {
            if (message.Notification == Messages.CloseVariableEditor)
                DialogHost.CloseDialogCommand.Execute(true, this);
        }
    }
}