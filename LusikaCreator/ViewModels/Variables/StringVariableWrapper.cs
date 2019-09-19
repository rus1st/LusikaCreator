using GalaSoft.MvvmLight;
using TestApp.Models;
using TestApp.Models.Enums;
using TestApp.Models.Interfaces;
using TestApp.Models.Variables;
using TestApp.Repository;
using TestApp.ViewModels.Interfaces;

namespace TestApp.ViewModels.Variables
{
    public class StringVariableWrapper : ViewModelBase, IVariableWrapper
    {
        private bool _isSubscribedToUpdates;
        private readonly VariablesRepository _variablesRepository;

        public event Handlers.EmptyHandler ValueChanged;

        public string Name
        {
            get { return _name; }
            set
            {
                if (_value == value) return;
                Set(ref _name, value);
            }
        }
        private string _name;

        public VariableType Type => VariableType.String;

        public bool IsAssigned { get; set; }

        public string Value
        {
            get { return _value; }
            set
            {
                if (_value == value) return;
                Set(ref _value, value);
                UpdateFormattedValue();
            }
        }
        private string _value;

        public string FormattedValue
        {
            get { return _formattedValue; }
            set
            {
                if (_formattedValue == value) return;
                Set(ref _formattedValue, value);
                StringValue = value;

                if (_isSubscribedToUpdates) ValueChanged?.Invoke();
            }
        }
        private string _formattedValue;

        public string StringValue
        {
            get { return _stringValue; }
            set { Set(ref _stringValue, value); }
        }
        private string _stringValue;


        public StringVariableWrapper(string name, VariablesRepository variablesRepository, bool isAssigned = false)
        {
            _variablesRepository = variablesRepository;
            Name = name;
            IsAssigned = isAssigned;
            SetDefault();
        }

        public void SubscribeToUpdates()
        {
            _isSubscribedToUpdates = true;
        }

        public void UnSubscribeToUpdates()
        {
            _isSubscribedToUpdates = false;
        }

        public void Set(object value)
        {
            if (!(value is string)) return;
            Value = (string) value;
        }

        public void SetDefault()
        {
            Value = string.Empty;
            SubscribeToUpdates();
        }

        public void UpdateFormattedValue()
        {
            FormattedValue = _variablesRepository.GetFormattedText(Value);
        }

        public bool IsContainTag(IVariableWrapper sourceVariable)
        {
            var tagName = "{#" + sourceVariable.Name + "}";
            return Value.Contains(tagName);
        }

        public void UpdateTag(IVariableWrapper sourceVariable)
        {
            var tagName = "{#" + sourceVariable.Name + "}";
            var replaceTo = sourceVariable.StringValue;
            FormattedValue = Value.Replace(tagName, replaceTo);
        }

        public bool IsEqualTo(IVariableWrapper compared)
        {
            if (!(compared is StringVariableWrapper)) return false;
            var variable = (StringVariableWrapper) compared;

            return Common.IsSameName(Name, variable.Name) &&
                   Value == variable.Value;
        }

        public void Update(IVariableWrapper buffer)
        {
            if (!(buffer is StringVariableWrapper)) return;
            var variable = (StringVariableWrapper) buffer;

            if (Name != variable.Name) Name = variable.Name;
            if (Value != variable.Value) Value = variable.Value;
        }

        public IVariableWrapper Clone()
        {
            return new StringVariableWrapper(Name, _variablesRepository)
            {
                _isSubscribedToUpdates = false,
                Value = Value
            };
        }

        public IVariable ToStoredObject()
        {
            return new StringVariable
            {
                Name = Name,
                Value = Value
            };
        }

        public override string ToString()
        {
            return FormattedValue;
        }
    }
}