using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using TestApp.Models;
using TestApp.Models.Config;
using TestApp.Models.Enums;
using TestApp.Repository;
using TestApp.ViewModels.Controls;
using TestApp.ViewModels.Helpers;
using TestApp.ViewModels.Interfaces;

namespace TestApp.ViewModels.Windows
{
    public class ObjectBrowserViewModel : ViewModelBase
    {
        private IObjectViewModel _selected;
        private readonly List<IObjectViewModel> _unsavedChanges;
        private readonly ObjectsRepository _objectsRepository;
        private readonly VariablesRepository _variablesRepository;
        private readonly DialogsManager _dialogsHelper;
        private readonly string _rootPath;
        
        public RelayCommand RemoveCommand => new RelayCommand(RemoveObject);
        public RelayCommand SaveChangesCommand => new RelayCommand(SaveChanges);
        public RelayCommand DiscardChangesCommand => new RelayCommand(DiscardChanges);
        public RelayCommand AddActionCommand => new RelayCommand(AddAction);
        public RelayCommand EditActionCommand => new RelayCommand(EditAction);
        public RelayCommand RemoveActionCommand => new RelayCommand(RemoveAction);

        public WindowSettings Settings { get; set; }

        public bool IsSet
        {
            get { return _isSet; }
            set { Set(ref _isSet, value); }
        }
        private bool _isSet;

        public uint Id => _selected?.Properties.Id ?? 0;

        public ObjectType Type
        {
            get
            {
                if (_selected is LabelViewModel) return ObjectType.Label;
                if (_selected is TextBoxViewModel) return ObjectType.TextBox;
                if (_selected is CheckBoxViewModel) return ObjectType.CheckBox;
                if (_selected is RadioViewModel) return ObjectType.RadioButton;
                if (_selected is DatePickerViewModel) return ObjectType.DatePicker;
                if (_selected is TimePickerViewModel) return ObjectType.TimePicker;
                return ObjectType.Unknown;
            }
        }

        public string Name
        {
            get { return _selected?.Properties.Name; }
            set
            {
                if (_selected == null) return;
                AnotherFieldsIsChanged = !Common.IsSameName(_selected.Properties.Name, value);
                _selected.Properties.Name = value;
                RaisePropertyChanged();
            }
        }

        public bool HasText
        {
            get { return _hasText; }
            set { Set(ref _hasText, value); }
        }
        private bool _hasText;

        public string Text
        {
            get
            {
                if (!(_selected is ITextProperties))
                {
                    HasText = false;
                    return null;
                }
                HasText = true;
                return ((ITextProperties) _selected).TextProperties.Text;
            }
            set
            {
                HasText = value != null;
                Set(ref _text, value);
                if (!(_selected is ITextProperties)) return;

                var vm = (ITextProperties)_selected;
                vm.TextProperties.Text = value;
                vm.TextProperties.FormattedText = _objectsRepository.ReplaceValuesInString(value);

                AnotherFieldsIsChanged = !Common.IsSameName(value, _selected.Properties.Name);
            }
        }
        private string _text;

        public bool IsVisible
        {
            get { return _selected?.Properties.GetVisibility() ?? false; }
            set
            {
                if (_selected == null) return;
                _selected.Properties.SetVisibility(value);

                AnotherFieldsIsChanged = true;
            }
        }

        public bool CanBeRequired
        {
            get
            {
                var ret = _selected is IRequired;
                return ret;
            }
        }

        public bool IsRequired
        {
            get
            {
                if (!(_selected is IRequired)) return false;
                return ((IRequired) _selected).IsRequired;
            }
            set
            {
                if (!(_selected is IRequired)) return;
                ((IRequired) _selected).IsRequired = value;
                RaisePropertyChanged();
                AnotherFieldsIsChanged = true;
            }
        }

        public bool IsMultiline
        {
            get { return _isMultiline; }
            set
            {
                if (_isMultiline == value) return;

                Set(ref _isMultiline, value);
                if (!(_selected is TextBoxViewModel)) return;
                ((TextBoxViewModel)_selected).IsMultiline = value;
                AnotherFieldsIsChanged = true;
            }
        }
        private bool _isMultiline;

        public bool IsRadioButton
        {
            get
            {
                if (!(_selected is RadioViewModel)) return false;

                RadioGroupName = ((RadioViewModel)_selected).GroupName;
                return true;
            }
        }

        public bool IsTextBox => _selected is TextBoxViewModel;

        public string RadioGroupName
        {
            get { return _radioGroupName; }
            set
            {
                Set(ref _radioGroupName, value);
                AnotherFieldsIsChanged = true;

                if (_selected.GetType() != typeof(RadioViewModel)) return;
                var radio = (RadioViewModel)_selected;
                radio.GroupName = value;
            }
        }
        private string _radioGroupName;

        #region Is changed

        public bool HasChanges
        {
            get { return _hasChanges; }
            set { Set(ref _hasChanges, value); }
        }
        private bool _hasChanges;

        public bool VariableIsChanged
        {
            get { return _variableIsChanged; }
            set
            {
                Set(ref _variableIsChanged, value);
                if (value && !HasChanges) HasChanges = true;
            }
        }
        private bool _variableIsChanged;

        public bool AnotherFieldsIsChanged
        {
            get { return _anotherFieldsIsChanged; }
            set
            {
                Set(ref _anotherFieldsIsChanged, value);
                if (value && !HasChanges) HasChanges = true;
            }
        }
        private bool _anotherFieldsIsChanged;

        #endregion

        #region Variable

        public ValuesSwitcherViewModel InvariantVariable { get; set; }

        public bool HasVariable
        {
            get { return _hasVariable; }
            set { Set(ref _hasVariable, value); }
        }
        private bool _hasVariable;

        #endregion

        #region Actions

        public ObservableCollection<ObjectAction> Actions { get; set; } = new ObservableCollection<ObjectAction>();

        public string ActionsTitle
        {
            get { return _actionsTitle; }
            set { Set(ref _actionsTitle, value); }
        }
        private string _actionsTitle;

        public ObjectAction SelectedAction
        {
            get { return _selectedAction; }
            set
            {
                Set(ref _selectedAction, value);
                ActonIsSelected = value != null;
            }
        }
        private ObjectAction _selectedAction;

        public bool ActonIsSelected
        {
            get { return _actionIsSelected; }
            set { Set(ref _actionIsSelected, value); }
        }
        private bool _actionIsSelected;

        private void ActionsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ActionsTitle = $"Действия{(Actions.Count > 0 ? $" ({Actions.Count})" : "")}";

            if (!(_selected is IActionProperties)) return;

            var actions = ((IActionProperties)_selected).ActionProperties.Actions;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    var addedAction = e.NewItems[0] as ObjectAction;
                    if (addedAction != null) actions.Add(addedAction);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    actions.Remove(e.OldItems[0] as ObjectAction);
                    break;

                case NotifyCollectionChangedAction.Replace:
                    throw new NotImplementedException();

                case NotifyCollectionChangedAction.Reset:
                    actions.Clear();
                    break;
            }
        }

        private async void AddAction()
        {
            if (!(_selected is IActionProperties)) return;

            var variable = ((IActionProperties)_selected).ActionProperties.Variable;

            var addedAction = await _dialogsHelper.OpenActionEditor(variable,
                Actions, _objectsRepository, _variablesRepository, _rootPath, WindowType.ObjectBrowser);

            if (addedAction == null) return;

            Actions.Add(addedAction);
            AnotherFieldsIsChanged = true;
        }

        private async void EditAction()
        {
            if (SelectedAction == null) return;

            var modifiedAction = await _dialogsHelper.OpenActionEditor(SelectedAction, Actions,
                _objectsRepository, _variablesRepository, WindowType.ObjectBrowser);

            if (modifiedAction != null) SelectedAction.Update(modifiedAction);
        }

        private async void RemoveAction()
        {
            if (!await _dialogsHelper.ShowRequest("Удалить выбранное действие?", WindowType.ObjectBrowser)) return;

            Actions.Remove(SelectedAction);
            SelectedAction = Actions.Count == 0 ? null : Actions[0];
            AnotherFieldsIsChanged = true;
        }

        #endregion


        public ObjectBrowserViewModel(DataProvider dataProvider)
        {
            _objectsRepository = dataProvider.ObjectsRepository;
            _variablesRepository = dataProvider.VariablesRepository;
            _dialogsHelper = dataProvider.DialogsManager;
            _rootPath = dataProvider.CommonSettings.RootPath;
            Settings = dataProvider.CommonSettings.ObjectBrowserSettings;
            InvariantVariable = new ValuesSwitcherViewModel(dataProvider, isCreateMode: false);
            HasVariable = false;

            _unsavedChanges = new List<IObjectViewModel>();
            _objectsRepository.SelectionChanged += SetSelected;
            _objectsRepository.Unselected += OnUnselected;

            InvariantVariable.VariableChanged += delegate { VariableIsChanged = true; };
            Actions.CollectionChanged += ActionsChanged;
            Actions.Clear();
        }



        private void SetSelected(IObjectViewModel viewModel)
        {
            if (viewModel == null)
            {
                // Сняли выделение
                if (_selected != null && HasChanges) AddToBuffer(_selected);
                _selected = null;
                IsSet = false;
                VariableIsChanged = false;
                AnotherFieldsIsChanged = false;
                return;
            }

            //_variablesRepository.VariableChanged -= UpdateVariable;
            //_variablesRepository.VariableChanged += UpdateVariable;
            var saved = _unsavedChanges.FirstOrDefault(t => t.Properties.Id == viewModel.Properties.Id);
            _selected = saved ?? viewModel.Clone();

            RaisePropertyChanged(nameof(Id));
            RaisePropertyChanged(nameof(Name));
            RaisePropertyChanged(nameof(Type));
            RaisePropertyChanged(nameof(Text));
            RaisePropertyChanged(nameof(IsVisible));
            RaisePropertyChanged(nameof(IsRequired));
            RaisePropertyChanged(nameof(CanBeRequired));
            RaisePropertyChanged(nameof(IsTextBox));
            RaisePropertyChanged(nameof(IsRadioButton));

            if (_selected is IActionProperties)
            {
                HasVariable = true;
                InvariantVariable.Variable = ((IActionProperties)_selected).ActionProperties.Variable;

                var vm = (IActionProperties)_selected;
                var actions = vm.ActionProperties.Actions.ToList();
                Actions.Clear();

                if (actions.Count > 0)
                    foreach (var action in actions) Actions.Add(action);
            }
            else
            {
                Actions.Clear();
                HasVariable = false;
            }

            IsSet = true;
            HasChanges = saved != null;
            _variableIsChanged = false;
        }

        private void AddToBuffer(IObjectViewModel selected)
        {
            if (_unsavedChanges.Count == 0)
            {
                _unsavedChanges.Add(selected);
                return;
            }

            var saved = _unsavedChanges.FirstOrDefault(t => t.Properties.Id == selected.Properties.Id);
            if (saved != null) _unsavedChanges.Remove(saved);

            _unsavedChanges.Add(selected);
        }

        /// <summary>
        /// Сняли выделение
        /// </summary>
        private void OnUnselected()
        {
            _objectsRepository.UnselectAll();
            SetSelected(null);
        }

        private async void SaveChanges()
        {
            //_variablesRepository.VariableChanged -= UpdateVariable;

            var hasErrors = !_objectsRepository.Update(_selected);
            if (hasErrors)
            {
                await _dialogsHelper.ShowMessage(_objectsRepository.ErrorMessage, WindowType.ObjectBrowser);
                return;
            }

            HasChanges = false;
            var selector = _unsavedChanges.FirstOrDefault(t => t.Properties.Id == _selected.Properties.Id);
            if (_unsavedChanges.FirstOrDefault(t => t.Properties.Id == _selected.Properties.Id) != null)
                _unsavedChanges.Remove(selector);
        }

        private void DiscardChanges()
        {
            HasChanges = false;
            var selector = _unsavedChanges.FirstOrDefault(t => t.Properties.Id == _selected.Properties.Id);
            if (selector != null) _unsavedChanges.Remove(selector);

            var viewModel = _objectsRepository.ViewModels.FirstOrDefault(t => t.Properties.Id == _selected.Properties.Id);
            if (viewModel == null)
            {
                IsSet = false;
                return;
            }
            SetSelected(viewModel);
        }

        private async void RemoveObject()
        {
            if (!await _dialogsHelper.ShowRequest("Удалить объект?", WindowType.ObjectBrowser))
                return;

            _objectsRepository.Remove(_selected.Properties.Id);
            SetSelected(null);
        }

    }
}