using System.Windows;
using System.Windows.Controls;
using GalaSoft.MvvmLight.Messaging;
using MaterialDesignThemes.Wpf;
using TestApp.ViewModels.Dialogs;
using TestApp.ViewModels.Helpers;

namespace TestApp.Views.Dialogs
{
    public partial class ScriptEditor : UserControl
    {
        #region ViewModel

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel",
            typeof(ScriptEditorViewModel),
            typeof(ScriptEditor));

        public ScriptEditorViewModel ViewModel
        {
            get { return (ScriptEditorViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        #endregion

        public ScriptEditor()
        {
            InitializeComponent();
            Messenger.Default.Register<NotificationMessage>(this, Close);
        }

        public void Close(NotificationMessage message)
        {
            if (message.Notification == Messages.DoCloseScriptEditor)
                DialogHost.CloseDialogCommand.Execute(true, this);
        }
    }
}