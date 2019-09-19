using System.Collections.Generic;
using System.Linq;
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
    public class RadioViewModel : ViewModelBase, IObjectViewModel, ITextProperties, ICheckable, IGrouped, IActionProperties
    {
        private readonly ObjectsRepository _objectsRepository;

        public ObjectBaseProperties Properties { get; set; }

        public ObjectTextProperties TextProperties { get; set; }

        public ObjectActionProperties ActionProperties { get; set; }

        public List<IVariableWrapper> RelatedVariables { get; set; } = new List<IVariableWrapper>();

        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                if (ActionProperties == null || IsChecked == value) return;
                Set(ref _isChecked, value);
                ((BoolVariableWrapper) ActionProperties.Variable).Set(value);
            }
        }
        private bool _isChecked;

        public string GroupName
        {
            get { return _groupName; }
            set
            {
                if (_groupName == value) return;
                Set(ref _groupName, value);
            }
        }
        private string _groupName;

        public RadioViewModel()
        {
        }

        public RadioViewModel(uint id, string name,
            IVariableWrapper variable,
            DataProvider dataProvider)
        {
            _objectsRepository = dataProvider.ObjectsRepository;
            Properties = new ObjectBaseProperties(id, name, dataProvider.CommonSettings.AppMode,
                dataProvider.ObjectsRepository);
            TextProperties = new ObjectTextProperties(dataProvider.VariablesRepository);
            ActionProperties = new ObjectActionProperties(variable, dataProvider.VariablesRepository,
                dataProvider.ObjectsRepository);
            ActionProperties.Variable.ValueChanged += OnValueChanged;
            IsChecked = ((BoolVariableWrapper) ActionProperties.Variable).IsSet;
        }

        private void OnValueChanged()
        {
            var value = ((BoolVariableWrapper) ActionProperties.Variable).IsSet;
            _objectsRepository.ProcessActions(this);

            if (Equals(_isChecked, value)) return;

            _isChecked = value;
            RaisePropertyChanged("IsChecked");
        }

        public void Update(IObjectViewModel buffer)
        {
            if (!(buffer is RadioViewModel)) return;
            var viewModel = (RadioViewModel) buffer;

            Properties.Update(viewModel.Properties);
            GroupName = viewModel.GroupName;
            TextProperties.Update(viewModel.TextProperties);
            ActionProperties.Update(viewModel.ActionProperties);
        }

        public IObjectViewModel Clone()
        {
            return new RadioViewModel
            {
                Properties = Properties.Clone(),
                GroupName = GroupName,
                TextProperties = TextProperties.Clone(),
                ActionProperties = ActionProperties.Clone()
            };
        }

        public IFormObject ToStoredObject()
        {
            return new RadioButtonObject
            {
                Id = Properties.Id,
                Name = Properties.Name,
                Left = Properties.Left,
                Top = Properties.Top,
                Text = TextProperties.Text,
                GroupName = GroupName,
                IsVisible = Properties.GetVisibility(),
                VariableName = ActionProperties.Variable.Name,
                Actions = ActionProperties.Actions.ToList(),
                FontSettings = Properties.FontSettings?.ToStoredObject(),
                TabId = Properties.TabId
            };
        }
    }
}