using System;
using GalaSoft.MvvmLight;
using TestApp.Models;
using TestApp.Models.Enums;
using TestApp.Models.Interfaces;
using TestApp.Models.Variables;
using TestApp.ViewModels.Interfaces;

namespace TestApp.ViewModels.Variables
{
    public class TimeVariableWrapper : ViewModelBase, IVariableWrapper
    {
        private bool _isSubscribedToUpdates;
        public event Handlers.EmptyHandler ValueChanged;

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

        public TimeSpan? Value
        {
            get { return _value; }
            set
            {
                Set(ref _value, value);
                StringValue = ToString();
                if (_isSubscribedToUpdates) ValueChanged?.Invoke();
            }
        }
        private TimeSpan? _value;

        public bool UseCurrentTime
        {
            get { return _useCurrentTime; }
            set
            {
                if (_useCurrentTime == value) return;
                Set(ref _useCurrentTime, value);
                if (value) Value = DateTime.Now.TimeOfDay;
            }
        }
        private bool _useCurrentTime;

        public bool UseSeconds
        {
            get { return _useSeconds; }
            set
            {
                if (_useSeconds == value) return;
                Set(ref _useSeconds, value);
            }
        }
        private bool _useSeconds;

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

        public string StringValue
        {
            get { return _stringValue; }
            set { Set(ref _stringValue, value); }
        }
        private string _stringValue;

        public VariableType Type => VariableType.Time;

        public bool IsAssigned { get; set; }


        public TimeVariableWrapper(string name, bool isAssigned = false)
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
            else Value = (TimeSpan) value;
        }

        public void SetDefault()
        {
            Value = null;
            UseCurrentTime = false;
            UseSeconds = false;
            Format = "t";
            SubscribeToUpdates();
        }

        public bool IsEqualTo(IVariableWrapper compared)
        {
            if (!(compared is TimeVariableWrapper)) return false;
            var variable = (TimeVariableWrapper) compared;

            return Common.IsSameName(Name, variable.Name) &&
                   Value == variable.Value;
        }

        public void Update(IVariableWrapper buffer)
        {
            if (!(buffer is TimeVariableWrapper)) return;
            var variable = (TimeVariableWrapper) buffer;

            if (Name != variable.Name) Name = variable.Name;
            if (Value != variable.Value) Value = variable.Value;
            if (UseSeconds != variable.UseSeconds) UseSeconds = variable.UseSeconds;
            if (UseCurrentTime != variable.UseCurrentTime) UseCurrentTime = variable.UseCurrentTime;
            if (Format != variable.Format) Format = variable.Format;
        }

        public IVariableWrapper Clone()
        {
            return new TimeVariableWrapper(Name)
            {
                _isSubscribedToUpdates = false,
                Value = Value,
                UseCurrentTime = UseCurrentTime,
                UseSeconds = UseSeconds,
                Format = Format
            };
        }

        public IVariable ToStoredObject()
        {
            return new TimeVariable
            {
                Name = Name,
                Value = UseCurrentTime ? null : Value,
                UseSeconds = UseSeconds,
                UseCurrentTime = UseCurrentTime,
                Format = Format
            };
        }

        public override string ToString()
        {
            var ret = Value?.ToString(Format) ?? string.Empty;
            return ret;
        }

        public string ToString(string format)
        {
            return Value?.ToString(format) ?? string.Empty;
        }
    }
}