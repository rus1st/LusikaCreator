using System;
using System.Linq;
using GalaSoft.MvvmLight;
using TestApp.Models;
using TestApp.Models.FormObjects;
using TestApp.Models.Interfaces;
using TestApp.ViewModels.Interfaces;
using TestApp.ViewModels.ObjectProperties;
using TestApp.ViewModels.Variables;

namespace TestApp.ViewModels.Controls
{
    public class TimePickerViewModel : ViewModelBase, IObjectViewModel, IActionProperties, IRequired
    {
        public ObjectBaseProperties Properties { get; set; }

        public ObjectTextProperties TextProperties { get; set; }

        public ObjectActionProperties ActionProperties { get; set; }

        public bool IsRequired
        {
            get { return _isRequired; }
            set { Set(ref _isRequired, value); }
        }
        private bool _isRequired;

        public bool IsComplete => !IsRequired || Time.HasValue;

        public void Refresh()
        {
            RaisePropertyChanged("TimeForBinding");
        }

        public object TimeForBinding
        {
            get { return _timeForBinding; }
            set
            {
                Set(ref _timeForBinding, value);
                if (value == null)
                {
                    Time = null;
                    return;
                }
                var val = (DateTime) value;
                Time = new TimeSpan(val.Hour, val.Minute, val.Second);
            }
        }
        private object _timeForBinding;

        public TimeSpan? Time
        {
            get { return _time; }
            set
            {
                if (ActionProperties == null || _time == value) return;
                Set(ref _time, value);
                ((TimeVariableWrapper) ActionProperties.Variable).Set(value);
            }
        }
        private TimeSpan? _time;

        public bool UseSeconds
        {
            get { return _useSeconds; }
            set
            {
                Set(ref _useSeconds, value);
                TimeFormat = value ? "Long" : "Short";
            }
        }
        private bool _useSeconds;

        public string TimeFormat
        {
            get { return _timeFormat; }
            set { Set(ref _timeFormat, value); }
        }
        private string _timeFormat;


        public TimePickerViewModel()
        {
        }

        public TimePickerViewModel(uint id, string name,
            IVariableWrapper variable,
            DataProvider dataProvider)
        {
            Properties = new ObjectBaseProperties(id, name, dataProvider.CommonSettings.AppMode,
                dataProvider.ObjectsRepository);
            TextProperties = new ObjectTextProperties(dataProvider.VariablesRepository);
            ActionProperties = new ObjectActionProperties(variable, dataProvider.VariablesRepository,
                dataProvider.ObjectsRepository);
            ActionProperties.Variable.ValueChanged += OnValueChanged;

            var timeWrapper = (TimeVariableWrapper) ActionProperties.Variable;
            Time = timeWrapper.Value;
            UpdateBindingTime();
            UseSeconds = timeWrapper.UseSeconds;
            IsRequired = false;
        }

        public void UpdateBindingTime()
        {
            if (Time == null) TimeForBinding = null;
            else
            {
                var t = Time.Value;
                TimeForBinding = new DateTime(1984, 1, 24, t.Hours, t.Minutes, t.Seconds);
            }
        }

        private void OnValueChanged()
        {
            var variable = (TimeVariableWrapper) ActionProperties.Variable;

            _time = variable.Value;
            UpdateBindingTime();
            //RaisePropertyChanged("TimeForBinding");
            UseSeconds = variable.UseSeconds;
        }

        public void Update(IObjectViewModel buffer)
        {
            if (!(buffer is TimePickerViewModel)) return;
            var viewModel = (TimePickerViewModel) buffer;

            Properties.Update(viewModel.Properties);
            TextProperties.Update(viewModel.TextProperties);
            //Time = viewModel.Time;
            ActionProperties.Update(viewModel.ActionProperties);
            IsRequired = viewModel.IsRequired;
        }

        public IObjectViewModel Clone()
        {
            return new TimePickerViewModel
            {
                Properties = Properties.Clone(),
                TextProperties = TextProperties.Clone(),
                //Time = Time,
                IsRequired = IsRequired,
                ActionProperties = ActionProperties.Clone()
            };
        }

        public IFormObject ToStoredObject()
        {
            return new TimePickerObject
            {
                Id = Properties.Id,
                Name = Properties.Name,
                Left = Properties.Left,
                Top = Properties.Top,
                IsVisible = Properties.GetVisibility(),
                IsRequired = IsRequired,
                VariableName = ActionProperties.Variable.Name,
                Actions = ActionProperties.Actions.ToList(),
                FontSettings = Properties.FontSettings?.ToStoredObject(),
                TabId = Properties.TabId
            };
        }
    }
}