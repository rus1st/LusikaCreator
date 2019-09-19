using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using TestApp.ViewModels.Helpers;

namespace TestApp.ViewModels.Dialogs
{
    public class MessageDialogViewModel : ViewModelBase
    {
        public string Text
        {
            get { return _text; }
            set { Set(ref _text, value); }
        }
        private string _text;

        public bool CancelBtnIsEnabled
        {
            get { return _cancelBtnIsEnabled; }
            set { Set(ref _cancelBtnIsEnabled, value); }
        }
        private bool _cancelBtnIsEnabled;

        public string OkBtnText
        {
            get { return _okBtnText; }
            set { Set(ref _okBtnText, value); }
        }
        private string _okBtnText;


        public MessageDialogViewModel(string text, bool isSimpleMessage = true)
        {
            Text = text;
            CancelBtnIsEnabled = !isSimpleMessage;
            OkBtnText = isSimpleMessage ? "Ok" : "Да";
        }

        public void CloseDialog()
        {
            Messenger.Default.Send(new NotificationMessage(Messages.DoCloseMessageDialog));
        }
    }
}