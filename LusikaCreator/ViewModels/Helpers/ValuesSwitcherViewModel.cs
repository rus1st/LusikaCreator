using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using TestApp.Models;
using TestApp.Models.Enums;
using TestApp.Repository;
using TestApp.ViewModels.Interfaces;
using TestApp.ViewModels.Variables;

namespace TestApp.ViewModels.Helpers
{
    public class FormattedDateTime
    {
        public string Date { get; set; }

        public string Format { get; set; }

        public FormattedDateTime(DateTime date, string format)
        {
            Format = format;
            Date = date.ToString(format);
        }

        public FormattedDateTime(TimeSpan time, string format)
        {
            Format = format;
            Date = time.ToString(format);
        }

        public override string ToString()
        {
            return Date;
        }
    }

    public class ValuesSwitcherViewModel : ViewModelBase
    {
        private readonly VariablesRepository _variablesRepository;
        private readonly DialogsManager _dialogsManager;

        public event Handlers.EmptyHandler VariableChanged;

        public RelayCommand ClearDateCommand => new RelayCommand(ClearDate);
        public RelayCommand ClearTimeCommand => new RelayCommand(ClearTime);
        public RelayCommand<string> CopyFormatCommand => new RelayCommand<string>(CopyFormat);

        public bool HasChanges
        {
            get { return _hasChanges; }
            set
            {
                Set(ref _hasChanges, value);
                if (value) VariableChanged?.Invoke();
            }
        }
        private bool _hasChanges;

        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { Set(ref _isEnabled, value); }
        }
        private bool _isEnabled;

        public IVariableWrapper Variable
        {
            get { return _variable; }
            set
            {
                Set(ref _variable, value);
                IsString = false;
                IsBool = false;
                IsDate = false;
                IsTime = false;
                IsEnabled = value != null;

                if (value == null) return;

                Name = value.Name;
                if (value is StringVariableWrapper)
                {
                    var variable = (StringVariableWrapper) value;
                    Type = VariableType.String;
                    IsString = true;
                    StringValue = variable.Value;
                }
                else if (value is BoolVariableWrapper)
                {
                    var variable = (BoolVariableWrapper) value;
                    Type = VariableType.Bool;
                    IsBool = true;
                    BoolValue = variable.Value;
                }
                else if (value is DateVariableWrapper)
                {
                    var variable = (DateVariableWrapper) value;
                    Type = VariableType.Date;
                    IsDate = true;
                    Date = variable.Value;
                    if (FormattedDates.Count == 0) MakeFormattedDates(DateTime.Today);
                    FormattedDate = FormattedDates.FirstOrDefault(t => t.Format == variable.Format) ?? FormattedDates[0];
                    UseCurrentDate = variable.UseCurrentDate;
                }
                else if (value is TimeVariableWrapper)
                {
                    var variable = (TimeVariableWrapper) value;
                    Type = VariableType.Time;
                    IsTime = true;

                    if (!variable.Value.HasValue) Time = null;
                    else
                    {
                        var val = variable.Value.Value;
                        Time = new DateTime(1984, 1, 24, val.Hours, val.Minutes, val.Seconds);
                        if (FormattedTimes.Any()) MakeFormattedTimes(val);
                        if (FormattedTimes.Any())
                            FormattedTime = FormattedTimes.FirstOrDefault(t => t.Format == variable.Format) ??
                                            FormattedTimes[0];
                    }
                    UseCurrentTime = variable.UseCurrentTime;
                }
                HasChanges = false;
            }
        }
        private IVariableWrapper _variable;

        #region Name

        public string Name
        {
            get { return _name; }
            set
            {
                Set(ref _name, value);
                _variable.Name = value;
                if (!HasChanges) HasChanges = true;
            }
        }
        private string _name;

        public string OldName
        {
            get { return _oldName; }
            set { Set(ref _oldName, value); }
        }
        private string _oldName;

        public bool NameIsChanged => Variable != null && OldName != null && !Common.IsSameName(Variable.Name, OldName);

        #endregion

        public bool IsCreateMode
        {
            get { return _isCreateMode; }
            set
            {
                Set(ref _isCreateMode, value);
                MinHeight = value ? 0 : 30;
            }
        }
        private bool _isCreateMode;

        public int MinHeight
        {
            get { return _minHeight; }
            set { Set(ref _minHeight, value); }
        }
        private int _minHeight;


        public List<VariableType> VariableTypes
            => Enum.GetValues(typeof(VariableType)).Cast<VariableType>()
                .Where(t => t != VariableType.Unknown).ToList();

        public VariableType Type
        {
            get { return _type; }
            set
            {
                if (_type == value) return;

                Set(ref _type, value);
                if (IsCreateMode) Variable = CreateVariable(_type);
                if (!HasChanges) HasChanges = true;
            }
        }
        private VariableType _type;


        #region String

        public bool IsString
        {
            get { return _isString; }
            set { Set(ref _isString, value); }
        }
        private bool _isString;

        public string StringValue
        {
            get { return _stringValue; }
            set
            {
                Set(ref _stringValue, value);
                if (_variable is StringVariableWrapper) ((StringVariableWrapper) _variable).Set(value);
                if (!HasChanges) HasChanges = true;
            }
        }
        private string _stringValue;

        public bool IsFormattedString
        {
            get { return _isFormattedString; }
            set { Set(ref _isFormattedString, value); }
        }
        private bool _isFormattedString;

        #endregion

        #region Bool

        public bool IsBool
        {
            get { return _isBool; }
            set { Set(ref _isBool, value); }
        }
        private bool _isBool;

        public ActionSelectorOperand BoolValue
        {
            get { return _boolValue; }
            set
            {
                Set(ref _boolValue, value);
                if (!(_variable is BoolVariableWrapper)) return;

                ((BoolVariableWrapper) _variable).Set(value);
                if (!HasChanges) HasChanges = true;
            }
        }
        private ActionSelectorOperand _boolValue;

        public List<ActionSelectorOperand> BoolValues
            => Enum.GetValues(typeof (ActionSelectorOperand)).Cast<ActionSelectorOperand>().ToList();

        #endregion

        #region Date

        public bool IsDate
        {
            get { return _isDate; }
            set { Set(ref _isDate, value); }
        }
        private bool _isDate;

        public DateTime? Date
        {
            get { return _date; }
            set
            {
                Set(ref _date, value);
                if (_variable is DateVariableWrapper) ((DateVariableWrapper)_variable).Set(value);
                MakeFormattedDates(value);
                if (!HasChanges) HasChanges = true;
            }
        }
        private DateTime? _date;

        public FormattedDateTime FormattedDate
        {
            get { return _formattedDate; }
            set
            {
                if (value == null) return;

                Set(ref _formattedDate, value);
                ((DateVariableWrapper) Variable).Format = value.Format;
                if (!HasChanges) HasChanges = true;
            }
        }
        private FormattedDateTime _formattedDate;

        public bool UseCurrentDate
        {
            get { return _useCurrentDate; }
            set
            {
                Set(ref _useCurrentDate, value);
                if (_variable is DateVariableWrapper) ((DateVariableWrapper)_variable).UseCurrentDate = value;
                if (!HasChanges) HasChanges = true;
            }
        }
        private bool _useCurrentDate;

        public ObservableCollection<FormattedDateTime> FormattedDates { get; set; } =
            new ObservableCollection<FormattedDateTime>();

        private void MakeFormattedDates(DateTime? value)
        {
            if (!value.HasValue) return;
            var date = value.Value;
            FormattedDates.Clear();
            FormattedDates.Add(new FormattedDateTime(date, "d")); // 07.04.2019
            FormattedDates.Add(new FormattedDateTime(date, "D")); // 7 апреля 2019 г.
            FormattedDates.Add(new FormattedDateTime(date, "dd")); // 07
            FormattedDates.Add(new FormattedDateTime(date, "%d")); // 7
            FormattedDates.Add(new FormattedDateTime(date, "MMMM")); // Апрель
            FormattedDates.Add(new FormattedDateTime(date, "MM")); // 04
            FormattedDates.Add(new FormattedDateTime(date, "%M")); // 4
            FormattedDates.Add(new FormattedDateTime(date, "yyyy")); // 2019
            FormattedDates.Add(new FormattedDateTime(date, "yyyy-MM-dd")); // 2019-04-07
        }

        public void ClearDate()
        {
            Date = null;
        }

        #endregion

        #region Time

        public bool IsTime
        {
            get { return _isTime; }
            set { Set(ref _isTime, value); }
        }
        private bool _isTime;

        public object Time
        {
            get { return _time; }
            set
            {
                Set(ref _time, value);
                if (!HasChanges) HasChanges = true;

                if (value == null)
                {
                    if (_variable is TimeVariableWrapper) ((TimeVariableWrapper)_variable).Set(null);
                    return;
                }

                var t = (DateTime) value;
                //if ((DateTime) _time == t) return;
                var time = new TimeSpan(t.Hour, t.Minute, t.Second);

                if (_variable is TimeVariableWrapper) ((TimeVariableWrapper) _variable).Set(time);
                MakeFormattedTimes(time);
                if (!HasChanges) HasChanges = true;
            }
        }
        private object _time;

        public FormattedDateTime FormattedTime
        {
            get { return _formattedTime; }
            set
            {
                if (value == null) return;
                Set(ref _formattedTime, value);
                ((TimeVariableWrapper) Variable).Format = value.Format;
                if (!HasChanges) HasChanges = true;
            }
        }
        private FormattedDateTime _formattedTime;

        public ObservableCollection<FormattedDateTime> FormattedTimes { get; set; } =
            new ObservableCollection<FormattedDateTime>();

        private void MakeFormattedTimes(TimeSpan time)
        {
            FormattedTimes.Clear();
            if (UseSeconds) FormattedTimes.Add(new FormattedDateTime(time, "t")); // 08:04:00
            FormattedTimes.Add(new FormattedDateTime(time, "hh':'mm")); // 08:04
            FormattedTimes.Add(new FormattedDateTime(time, "hh")); // 08
            FormattedTimes.Add(new FormattedDateTime(time, "%h")); // 8
            FormattedTimes.Add(new FormattedDateTime(time, "mm")); // 04
            FormattedTimes.Add(new FormattedDateTime(time, "%m")); // 4
            FormattedTimes.Add(new FormattedDateTime(time, "ss")); // 00
            FormattedTimes.Add(new FormattedDateTime(time, "%s")); // 0
        }

        public bool UseSeconds
        {
            get { return _useSeconds; }
            set
            {
                Set(ref _useSeconds, value);
                if (_variable is TimeVariableWrapper) ((TimeVariableWrapper) _variable).UseSeconds = value;
                if (!HasChanges) HasChanges = true;
            }
        }
        private bool _useSeconds;

        public bool UseCurrentTime
        {
            get { return _useCurrentTime; }
            set
            {
                Set(ref _useCurrentTime, value);
                if (_variable is TimeVariableWrapper) ((TimeVariableWrapper) _variable).UseCurrentTime = value;
                if (!HasChanges) HasChanges = true;
            }
        }
        private bool _useCurrentTime;

        public void ClearTime()
        {
            Time = null;
        }

        #endregion


        public ValuesSwitcherViewModel(DataProvider dataProvider, IVariableWrapper variable = null,
            bool isCreateMode = true, bool isSubscribedToUpdates = false)
        {
            _variablesRepository = dataProvider.VariablesRepository;
            _dialogsManager = dataProvider.DialogsManager;

            if (variable == null)
            {
                IsEnabled = false;
                IsCreateMode = isCreateMode;
                Variable = CreateVariable(VariableType.String);
            }
            else
            {
                IsEnabled = true;
                IsCreateMode = isCreateMode;
                Variable = variable.Clone();
                OldName = Variable.Name;
            }

            HasChanges = false;
        }

        private IVariableWrapper CreateVariable(VariableType type)
        {
            var name = _variablesRepository.MakeName();

            switch (type)
            {
                case VariableType.String:
                    return new StringVariableWrapper(name, _variablesRepository);

                case VariableType.Bool:
                    return new BoolVariableWrapper(name);

                case VariableType.Date:
                    return new DateVariableWrapper(name);

                case VariableType.Time:
                    return new TimeVariableWrapper(name);
            }

            return null;
        }

        public void CopyFormat(string param)
        {
            string text = null;
            if (param == "date") text = FormattedDate.Format;
            else if (param == "time") text = FormattedTime.Format;
            if (text == null) return;

            Clipboard.SetText(text);
            _dialogsManager.ShowNotification("Значение скопировано в буфер обмена");
        }
    }
}