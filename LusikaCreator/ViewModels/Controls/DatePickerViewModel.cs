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
    public class DatePickerViewModel : ViewModelBase, IObjectViewModel, IActionProperties, IRequired
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

        public bool IsComplete
        {
            get
            {
                if (!IsRequired) return true;
                return Date.HasValue;
            }
        }

        public void Refresh()
        {
            RaisePropertyChanged("Date");
        }

        public DateTime? Date
        {
            get { return _date; }
            set
            {
                Set(ref _date, value);
                ((DateVariableWrapper) ActionProperties?.Variable)?.Set(value);
            }
        }
        private DateTime? _date;

        public DatePickerViewModel()
        {
        }

        public DatePickerViewModel(uint id, string name,
            IVariableWrapper variable,
            DataProvider dataProvider)
        {
            Properties = new ObjectBaseProperties(id, name, dataProvider.CommonSettings.AppMode,
                dataProvider.ObjectsRepository);
            TextProperties = new ObjectTextProperties(dataProvider.VariablesRepository);
            ActionProperties = new ObjectActionProperties(variable, dataProvider.VariablesRepository,
                dataProvider.ObjectsRepository);
            ActionProperties.Variable.ValueChanged += OnValueChanged;
            Date = ((DateVariableWrapper) ActionProperties.Variable).Value;
            IsRequired = false;
        }

        private void OnValueChanged()
        {
            var value = ((DateVariableWrapper) ActionProperties.Variable).Value;
            if (_date == value) return;

            _date = value;
            RaisePropertyChanged("Date");
        }

        public void Update(IObjectViewModel buffer)
        {
            if (!(buffer is DatePickerViewModel)) return;
            var viewModel = (DatePickerViewModel) buffer;

            Properties.Update(viewModel.Properties);
            TextProperties.Update(viewModel.TextProperties);
            ActionProperties.Update(viewModel.ActionProperties);
            IsRequired = viewModel.IsRequired;
        }

        public IObjectViewModel Clone()
        {
            return new DatePickerViewModel
            {
                Properties = Properties.Clone(),
                TextProperties = TextProperties.Clone(),
                IsRequired = IsRequired,
                ActionProperties = ActionProperties.Clone()
            };
        }

        public IFormObject ToStoredObject()
        {
            return new DateBoxObject
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