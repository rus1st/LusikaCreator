using System;
using System.Collections.Generic;
using GalaSoft.MvvmLight;
using TestApp.Models;
using TestApp.Models.Enums;
using TestApp.Models.Interfaces;
using TestApp.Models.Variables;
using TestApp.ViewModels.Interfaces;

namespace TestApp.ViewModels.Variables
{
    public class DateVariableWrapper : ViewModelBase, IVariableWrapper
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

        public DateTime? Value
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
        private DateTime? _value;

        public VariableType Type => VariableType.Date;

        public bool IsAssigned { get; set; }

        public string StringValue
        {
            get { return _stringValue; }
            set { Set(ref _stringValue, value); }
        }
        private string _stringValue;

        public bool UseCurrentDate
        {
            get { return _useCurrentDate; }
            set
            {
                if (_useCurrentDate == value) return;
                Set(ref _useCurrentDate, value);
                if (value) Value = DateTime.Today;
            }
        }
        private bool _useCurrentDate;

        public string Format
        {
            get { return _format; }
            set
            {
                if (_format == value) return;
                Set(ref _format, value);
                StringValue = ToString();
            }
        }
        private string _format;


        public DateVariableWrapper(string name, bool isAssigned = false)
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
            if (value == null) Value = null;
            else if (value is DateTime) Value = (DateTime) value;
        }

        public void SetDefault()
        {
            Value = DateTime.Today;
            UseCurrentDate = false;
            Format = "d";
            SubscribeToUpdates();
        }

        public bool IsEqualTo(IVariableWrapper compared)
        {
            if (!(compared is DateVariableWrapper)) return false;
            var variable = (DateVariableWrapper) compared;

            return Common.IsSameName(Name, variable.Name) &&
                   UseCurrentDate == variable.UseCurrentDate &&
                   Value == variable.Value;
        }

        public void Update(IVariableWrapper buffer)
        {
            if (!(buffer is DateVariableWrapper)) return;
            var variable = (DateVariableWrapper) buffer;

            if (Name != variable.Name) Name = variable.Name;
            if (Value != variable.Value) Value = variable.Value;
            if (UseCurrentDate != variable.UseCurrentDate) UseCurrentDate = variable.UseCurrentDate;
            if (Format != variable.Format) Format = variable.Format;
        }

        public IVariableWrapper Clone()
        {
            return new DateVariableWrapper(Name)
            {
                _isSubscribedToUpdates = false,
                Value = Value,
                UseCurrentDate = UseCurrentDate,
                Format = Format,
            };
        }

        public IVariable ToStoredObject()
        {
            return new DateVariable
            {
                Name = Name,
                UseCurrentDate = UseCurrentDate,
                Value = UseCurrentDate ? null : Value,
                Format = Format
            };
        }

        public override string ToString()
        {
            return Value?.ToString(Format) ?? string.Empty;
        }

        public string ToString(string format)
        {
            return Value?.ToString(format) ?? string.Empty;
        }

    }
}