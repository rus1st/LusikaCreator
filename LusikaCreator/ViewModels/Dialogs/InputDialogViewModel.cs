using GalaSoft.MvvmLight;

namespace TestApp.ViewModels.Dialogs
{
    public class InputDialogViewModel : ViewModelBase
    {
        public string Text
        {
            get { return _text; }
            set { Set(ref _text, value); }
        }
        private string _text;

        public string Value
        {
            get { return _value; }
            set { Set(ref _value, value); }
        }
        private string _value;

        public InputDialogViewModel(string text)
        {
            Text = text;
            Value = null;
        }
    }
}