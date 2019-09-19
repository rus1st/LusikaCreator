using GalaSoft.MvvmLight;
using TestApp.Repository;

namespace TestApp.ViewModels.ObjectProperties
{
    /// <summary>
    /// Определяет текстовые поля на объекте
    /// </summary>
    public class ObjectTextProperties : ViewModelBase
    {
        private readonly VariablesRepository _variablesRepository;

        public string Text
        {
            get { return _text; }
            set
            {
                if (_text == value) return;
                Set(ref _text, value);
                UpdateFormattedText();
            }
        }
        private string _text;

        public string FormattedText
        {
            get { return _formattedText; }
            set
            {
                if (_formattedText == value) return;
                Set(ref _formattedText, value);
            }
        }
        private string _formattedText;

        public ObjectTextProperties(VariablesRepository variablesRepository)
        {
            _variablesRepository = variablesRepository;
            SetDefault();
        }

        public void UpdateFormattedText()
        {
            FormattedText = _variablesRepository.GetFormattedText(Text);
        }

        public void Update(ObjectTextProperties buffer)
        {
            if (buffer == null) return;
            Text = buffer.Text;
        }

        public void SetDefault()
        {
            Text = "Текст";
        }

        public ObjectTextProperties Clone()
        {
            return new ObjectTextProperties(_variablesRepository) {Text = Text};
        }
    }
}