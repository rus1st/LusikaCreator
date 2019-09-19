using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using TestApp.ViewModels.Helpers;

namespace TestApp.ViewModels.Dialogs
{
    public class ProgressDialogViewModel : ViewModelBase
    {
        public string Text
        {
            get { return _text; }
            set { Set(ref _text, value); }
        }
        private string _text;

        public ProgressDialogViewModel(string text)
        {
            Text = text;
            Messenger.Default.Register<NotificationMessage<string>>(this, ChangeText);
        }

        private void ChangeText(NotificationMessage<string> message)
        {
            if (message.Notification != Messages.DoChangeProgressText) return;

            var text = message.Content;
            Text = text;
        }

    }
}