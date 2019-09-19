using System.Collections.Generic;
using GalaSoft.MvvmLight;
using TestApp.Models;
using TestApp.Models.Enums;
using TestApp.Models.Interfaces;
using TestApp.Models.Variables;
using TestApp.ViewModels.Interfaces;

namespace TestApp.ViewModels.Variables
{
    public class BoolVariableWrapper : ViewModelBase, IVariableWrapper
    {
        private bool _isSubscribedToUpdates;
        public event Handlers.EmptyHandler ValueChanged;

        public List<IVariableWrapper> DependedVariables { get; set; } = new List<IVariableWrapper>();
        public List<IObjectViewModel> DependedObjects { get; set; } = new List<IObjectViewModel>();
        public List<IVariableWrapper> RelatedVariables { get; set; } = new List<IVariableWrapper>();

        public string Name
        {
            get { return _name; }
            set
            {
                if (_name == value) return;
                Set(ref _name, value);
            }
        }
        private string _name;

        public ActionSelectorOperand Value
        {
            get { return _value; }
            set
            {
                if (_value == value) return;
                Set(ref _value, value);
                StringValue = ToString();

                if (_isSubscribedToUpdates) ValueChanged?.Invoke();
            }
        }
        private ActionSelectorOperand _value;

        public VariableType Type => VariableType.Bool;

        public bool IsAssigned { get; set; }

        public bool IsSet => Value == ActionSelectorOperand.Set;

        public string StringValue
        {
            get { return _stringValue; }
            set { Set(ref _stringValue, value); }
        }
        private string _stringValue;


        public BoolVariableWrapper(string name, bool isAssigned = false)
        {
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
            if (value is bool)
            {
                Value = (bool) value ? ActionSelectorOperand.Set : ActionSelectorOperand.NotSet;
            }
            else if (value is ActionSelectorOperand)
            {
                Value = (ActionSelectorOperand) value;
            }
        }

        public void SetDefault()
        {
            Value = ActionSelectorOperand.Set;
            SubscribeToUpdates();
        }

        public bool IsEqualTo(IVariableWrapper compared)
        {
            if (!(compared is BoolVariableWrapper)) return false;
            var variable = (BoolVariableWrapper) compared;

            return Common.IsSameName(Name, variable.Name) &&
                   Value == variable.Value;
        }

        public void Update(IVariableWrapper buffer)
        {
            if (!(buffer is BoolVariableWrapper)) return;
            var variable = (BoolVariableWrapper) buffer;

            if (Name != variable.Name) Name = variable.Name;
            if (Value != variable.Value) Value = variable.Value;
        }

        public IVariableWrapper Clone()
        {
            return new BoolVariableWrapper(Name)
            {
                _isSubscribedToUpdates = false,
                Value = Value
            };
        }

        public override string ToString()
        {
            return IsSet
                ? Common.GetEnumDescription(ActionSelectorOperand.Set)
                : Common.GetEnumDescription(ActionSelectorOperand.NotSet);
        }


        public IVariable ToStoredObject()
        {
            return new BoolVariable
            {
                Name = Name,
                Value = Value
            };
        }

        public string ToString(bool value)
        {
            return value
                ? Common.GetEnumDescription(ActionSelectorOperand.Set)
                : Common.GetEnumDescription(ActionSelectorOperand.NotSet);
        }
    }
}