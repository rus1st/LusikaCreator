using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GalaSoft.MvvmLight;
using TestApp.Models;
using TestApp.Models.FormObjects;
using TestApp.Models.Interfaces;
using TestApp.Repository;
using TestApp.ViewModels.Interfaces;
using TestApp.ViewModels.ObjectProperties;
using TestApp.ViewModels.Variables;

namespace TestApp.ViewModels.Controls
{
    public class TextBoxViewModel : ViewModelBase, IObjectViewModel, IActionProperties, IRequired
    {
        private readonly ObjectsRepository _objectsRepository;

        public ObjectBaseProperties Properties { get; set; }

        public ObjectActionProperties ActionProperties { get; set; }

        public bool IsRequired
        {
            get { return _isRequired; }
            set
            {
                Set(ref _isRequired, value);
                RaisePropertyChanged("Text");
            }
        }
        private bool _isRequired;

        public bool IsComplete
        {
            get
            {
                if (!IsRequired) return true;
                return !string.IsNullOrEmpty(Text);
            }
        }

        public void Refresh()
        {
            RaisePropertyChanged("Text");
        }

        public string Text
        {
            get { return _text; }
            set
            {
                if (_text == value) return;
                Set(ref _text, value);

                ((StringVariableWrapper) ActionProperties?.Variable)?.Set(value);
            }
        }
        private string _text;

        #region Multiline options

        public bool IsMultiline
        {
            get { return _isMultiline; }
            set
            {
                if (_isMultiline == value) return;
                Set(ref _isMultiline, value);

                if (value)
                {
                    ScroolIsVisible = ScrollBarVisibility.Auto;
                    IsWrapped = TextWrapping.Wrap;
                }
                else
                {
                    ScroolIsVisible = ScrollBarVisibility.Hidden;
                    IsWrapped = TextWrapping.NoWrap;
                }
            }
        }
        private bool _isMultiline;

        public ScrollBarVisibility ScroolIsVisible
        {
            get { return _scroolIsVisible; }
            set { Set(ref _scroolIsVisible, value); }
        }
        private ScrollBarVisibility _scroolIsVisible;

        public TextWrapping IsWrapped
        {
            get { return _isWrapped; }
            set { Set(ref _isWrapped, value); }
        }
        private TextWrapping _isWrapped;

        #endregion

        public TextBoxViewModel()
        {
        }

        public TextBoxViewModel(uint id, string name,
            IVariableWrapper variable,
            DataProvider dataProvider)
        {
            _objectsRepository = dataProvider.ObjectsRepository;
            Properties = new ObjectBaseProperties(id, name, dataProvider.CommonSettings.AppMode,
                dataProvider.ObjectsRepository);
            ActionProperties = new ObjectActionProperties(variable, dataProvider.VariablesRepository,
                dataProvider.ObjectsRepository);
            ActionProperties.Variable.ValueChanged += OnValueChanged;
            Text = ActionProperties.Variable.StringValue;
            IsMultiline = false;
            IsRequired = false;
        }

        private void OnValueChanged()
        {
            var value = ((StringVariableWrapper)ActionProperties?.Variable)?.FormattedValue;
            _objectsRepository.ProcessActions(this);

            if (_text == value) return;
            _text = value;
            RaisePropertyChanged("Text");
        }

        public void Update(IObjectViewModel buffer)
        {
            if (!(buffer is TextBoxViewModel)) return;
            var viewModel = (TextBoxViewModel)buffer;

            Properties.Update(viewModel.Properties);
            Text = viewModel.Text;
            IsMultiline = viewModel.IsMultiline;
            IsRequired = viewModel.IsRequired;
            ActionProperties.Update(viewModel.ActionProperties);
        }

        public IObjectViewModel Clone()
        {
            return new TextBoxViewModel
            {
                Properties = Properties.Clone(),
                Text = Text,
                IsMultiline = IsMultiline,
                IsRequired = IsRequired,
                ActionProperties = ActionProperties.Clone()
            };
        }

        public IFormObject ToStoredObject()
        {
            return new TextBoxObject
            {
                Id = Properties.Id,
                Name = Properties.Name,
                Left = Properties.Left,
                Top = Properties.Top,
                IsVisible = Properties.GetVisibility(),
                IsMultiLine = IsMultiline,
                IsRequired = IsRequired,
                VariableName = ActionProperties.Variable.Name,
                Actions = ActionProperties.Actions.ToList(),
                FontSettings = Properties.FontSettings?.ToStoredObject(),
                TabId = Properties.TabId
            };
        }
    }
}