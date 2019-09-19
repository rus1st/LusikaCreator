using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using TestApp.Models;
using TestApp.Models.Enums;
using TestApp.Models.Helpers;
using TestApp.ViewModels.Helpers;
using TestApp.ViewModels.Interfaces;
using TestApp.ViewModels.Variables;

namespace TestApp.ViewModels.Dialogs
{
    public class ActionEditorViewModel : ViewModelBase
    {
        private readonly bool _isInEditMode;
        private readonly IEnumerable<ObjectAction> _actions;
        private readonly Repository.ObjectsRepository _objectsRepository;
        private readonly Repository.VariablesRepository _variablesRepository;
        private readonly DialogsManager _dialogsHelper;
        private readonly ScriptRunner _scriptRunner;

        public RelayCommand AddConditionCommand => new RelayCommand(AddCondition);
        public RelayCommand RemoveConditionCommand => new RelayCommand(RemoveCondition);
        public RelayCommand AddVariableCommand => new RelayCommand(AddVariable);
        public RelayCommand SaveCommand => new RelayCommand(Save);
        public RelayCommand DiscardChangesCommand => new RelayCommand(DiscardChanges);

        public string Title
        {
            get { return _title; }
            set { Set(ref _title, value); }
        }
        private string _title;

        public bool HasChanges
        {
            get { return _hasChanges; }
            set { Set(ref _hasChanges, value); }
        }
        private bool _hasChanges;

        public bool IsSaved { get; set; }

        public ObjectAction Action
        {
            get { return _action; }
            set { Set(ref _action, value); }
        }
        private ObjectAction _action;

        #region Condition

        public bool HasCondition => Action?.Condition != null;

        public bool IsConditionHasInput
        {
            get { return _isConditionHasInput; }
            set { Set(ref _isConditionHasInput, value); }
        }
        private bool _isConditionHasInput;

        public ObservableCollection<object> ConditionOperands { get; set; } = new ObservableCollection<object>();

        public object ConditionValue
        {
            get { return Action.Condition.Value; }
            set
            {
                if (Action.Condition.Value == value) return;
                Action.Condition.Value = value;
                RaisePropertyChanged();
                if (!HasChanges) HasChanges = true;
            }
        }

        public object ConditionOperand
        {
            get { return _conditionOperand; }
            set
            {
                if (_conditionOperand == value) return;
                Set(ref _conditionOperand, value);

                if (!HasChanges) HasChanges = true;
                if (Action.Condition == null) Action.Condition = new ActionCondition();
                Action.Condition.Operand = value;

                IsConditionHasInput = Action.Condition.Operand.GetType() == typeof (ActionInputOperand) &&
                                      (ActionInputOperand) Action.Condition.Operand != ActionInputOperand.Set &&
                                      (ActionInputOperand) Action.Condition.Operand != ActionInputOperand.NotSet;

                ConditionOperandType = ConditionOperands.Count == 0 ? typeof (string) : ConditionOperands[0].GetType();
                HasChanges = true;
            }
        }
        private object _conditionOperand;

        public Type ConditionOperandType { get; set; }

        #endregion

        public List<ActionOperation> OperationNames
            => Enum.GetValues(typeof (ActionOperation)).Cast<ActionOperation>()
            
            
            .Where(t=>t!= ActionOperation.CallFunction)
            
            
            
            
            .ToList();

        private void UpdateTargetNames()
        {
            TargetNames.Clear();
            switch (Operation)
            {
                case ActionOperation.SetValue:
                    _variablesRepository.GetNames(Action.Variable.Name).ForEach(t => TargetNames.Add(t));
                    break;

                case ActionOperation.SetVisibility:
                    _objectsRepository.GetNames().ForEach(t => TargetNames.Add(t));
                    break;

                case ActionOperation.SetObjectText:
                    _objectsRepository.GetNamesOfObjectsWithText().ForEach(t => TargetNames.Add(t));
                    break;

                case ActionOperation.CallFunction:
                    //Enum.GetValues(typeof (InternalFunctionType))
                    //    .Cast<InternalFunctionType>()
                    //    .ToList()
                    //    .ForEach(t => TargetNames.Add(t.ToString()));
                    break;
            }
            TargetIsEmpty = TargetNames.Count == 0;
            TargetName = _isInEditMode
                ? Action.Result.TargetName
                : TargetIsEmpty ? null : TargetNames[0];
        }

        public ActionOperation Operation
        {
            get { return _operation; }
            set
            {
                if (_operation == value) return;

                Set(ref _operation, value);
                if (!_isInEditMode) Action.Result.Operation = value;

                IsVariableBtnEnabled = false;
                ValueIsList = false;
                ValueIsString = false;
                ValueIsDate = false;
                ResultLabelText = "Значение:";
                TargetNames.Clear();
                SelectableValues.Clear();

                switch (value)
                {
                    case ActionOperation.SetValue:
                        TargetLabelText = "Переменная:";
                        _variablesRepository.GetNames(Action.Variable.Name).ForEach(t => TargetNames.Add(t));
                        IsVariableBtnEnabled = true;
                        break;

                    case ActionOperation.SetVisibility:
                        TargetLabelText = "Объект:";
                        _objectsRepository.GetNames().ForEach(t => TargetNames.Add(t));

                        ValueIsList = true;
                        //SelectableValues.Clear();
                        Enum.GetValues(typeof (ActionVisibilityOperand))
                            .Cast<ActionVisibilityOperand>()
                            .ToList()
                            .ForEach(t => SelectableValues.Add(t));

                        RaisePropertyChanged("SelectableValues");
                        break;

                    case ActionOperation.SetObjectText:
                        TargetLabelText = "Объект:";
                        _objectsRepository.GetNamesOfObjectsWithText().ForEach(t => TargetNames.Add(t));

                        ValueIsString = true;
                        break;

                    case ActionOperation.CallFunction:
                        TargetLabelText = "Записать значение в переменную:";
                        ResultLabelText = "Метод:";

                        var funcNames = _scriptRunner.FuncNames;
                        if (funcNames != null)
                        {
                            funcNames.ForEach(t => SelectableValues.Add(t));
                            ValueIsList = true;
                        }

                        _variablesRepository.GetNames(Action.Variable.Name).ForEach(t => TargetNames.Add(t));
                        IsVariableBtnEnabled = true;
                        RaisePropertyChanged("SelectableValues");
                        break;
                }

                TargetIsEmpty = TargetNames.Count == 0;
                if (_isInEditMode && _variablesRepository.IsExists(Action.Result.TargetName))
                    TargetName = Action.Result.TargetName;
                else TargetName = TargetIsEmpty ? null : TargetNames[0];

                if (!HasChanges) HasChanges = true;
            }
        }
        private ActionOperation _operation;

        public ObservableCollection<string> TargetNames { get; set; }
            = new ObservableCollection<string>();

        public bool TargetIsEmpty
        {
            get { return _targetIsEmpty; }
            set { Set(ref _targetIsEmpty, value); }
        }
        private bool _targetIsEmpty;

        public string TargetLabelText
        {
            get { return _targetLabelText; }
            set { Set(ref _targetLabelText, value); }
        }
        private string _targetLabelText;

        public string ResultLabelText
        {
            get { return _resultLabelText; }
            set { Set(ref _resultLabelText, value); }
        }
        private string _resultLabelText;

        public object ResultValue
        {
            get { return Action.Result.Value; }
            set
            {
                if (Action.Result.Value == value) return;

                Action.Result.Value = value;
                RaisePropertyChanged();
                if (!HasChanges) HasChanges = true;
            }
        }

        public string TargetName
        {
            get { return _targetName; }
            set
            {
                if (_targetName == value) return;

                Set(ref _targetName, value);
                ValueIsList = false;
                ValueIsString = false;
                ValueIsDate = false;

                if (value == null) return;

                switch (Operation)
                {
                    case ActionOperation.SetValue:
                        var targetVariable = _variablesRepository.Find(value);
                        if (targetVariable == null)
                        {
                            // Переменную успели удалить
                            TargetNames.Remove(TargetName);
                            TargetName = TargetNames.Count > 0 ? TargetNames[0] : null;
                            return;
                        }

                        if (!_isInEditMode) Action.Result.TargetName = targetVariable.Name;
                        SelectableValues.Clear();

                        if (targetVariable is StringVariableWrapper)
                        {
                            ValueIsString = true;
                            ResultValue = _isInEditMode
                                ? (string) Action.Result.Value
                                : ((StringVariableWrapper) targetVariable).Value;
                        }
                        else if (targetVariable is BoolVariableWrapper)
                        {
                            Enum.GetValues(typeof (ActionSelectorOperand))
                                .Cast<ActionSelectorOperand>()
                                .ToList()
                                .ForEach(t => SelectableValues.Add(t));

                            ValueIsList = true;
                            SelectableValue = ((BoolVariableWrapper) targetVariable).Value;
                            ResultValue = _isInEditMode
                                ? (ActionSelectorOperand) Action.Result.Value
                                : ((BoolVariableWrapper) targetVariable).Value;
                            RaisePropertyChanged("SelectableValues");
                        }
                        else if (targetVariable is DateVariableWrapper)
                        {
                            ValueIsDate = true;
                            ResultValue = _isInEditMode
                                ? (DateTime?) Action.Result.Value
                                : ((DateVariableWrapper) targetVariable).Value;
                        }
                        else if (targetVariable is TimeVariableWrapper)
                        {
                            ResultValue = _isInEditMode
                                ? (TimeSpan) Action.Result.Value
                                : ((TimeVariableWrapper) targetVariable).Value;
                        }
                        break;

                    case ActionOperation.SetVisibility:
                        if (!_isInEditMode) Action.Result.TargetName = value;

                        var targetObject = _objectsRepository.Find(value);
                        if (targetObject == null) return;

                        ValueIsList = true;

                        var isVisible = IsInDesignMode
                            ? (bool) Action.Result.Value
                            : targetObject.Properties.GetVisibility();

                        SelectableValue = isVisible ? ActionVisibilityOperand.Show : ActionVisibilityOperand.Hide;
                        break;

                    case ActionOperation.SetObjectText:
                        if (!_isInEditMode) Action.Result.TargetName = value;

                        ValueIsString = true;
                        ResultValue = _isInEditMode
                            ? Action.Result.Value
                            : _objectsRepository.GetObjectText(TargetName);
                        break;

                    case ActionOperation.CallFunction:
                        ValueIsList = true;
                        if (SelectableValues != null && SelectableValues.Count > 0)
                            SelectableValue = SelectableValues[0];
                        else SelectableValue = null;
                        break;
                }
                if (!HasChanges) HasChanges = true;
            }
        }
        private string _targetName;

        public bool IsVariableBtnEnabled
        {
            get { return _isVariableBtnEnabled; }
            set { Set(ref _isVariableBtnEnabled, value); }
        }
        private bool _isVariableBtnEnabled;

        public bool ValueIsList
        {
            get { return _valueIsList; }
            set { Set(ref _valueIsList, value); }
        }
        private bool _valueIsList;

        public bool ValueIsString
        {
            get { return _valueIsString; }
            set { Set(ref _valueIsString, value); }
        }
        private bool _valueIsString;

        public bool ValueIsDate
        {
            get { return _valueIsDate; }
            set { Set(ref _valueIsDate, value); }
        }
        private bool _valueIsDate;

        public ObservableCollection<object> SelectableValues { get; set; }
            = new ObservableCollection<object>();

        public Type SelectableValueType
        {
            get { return _selectableValueType; }
            set { Set(ref _selectableValueType, value); }
        }
        private Type _selectableValueType;

        public object SelectableValue
        {
            get { return _selectableValue; }
            set
            {
                if (_selectableValue == value) return;

                Set(ref _selectableValue, value);
                SelectableValueType = value?.GetType();
                ResultValue = value;
                if (!HasChanges) HasChanges = true;
            }
        }
        private object _selectableValue;

        public IVariableWrapper Variable
        {
            get { return _variable; }
            set { Set(ref _variable, value); }
        }
        private IVariableWrapper _variable;


        public ActionEditorViewModel(
            ObjectAction selectedAction,
            IEnumerable<ObjectAction> actions,
            DataProvider dataProvider)
        {
            _isInEditMode = true;
            _actions = actions;
            _objectsRepository = dataProvider.ObjectsRepository;
            _variablesRepository = dataProvider.VariablesRepository;
            _dialogsHelper = dataProvider.DialogsManager;
            Action = selectedAction;
            Title = "Редактирование действия";
            Init();
            Update();
            //HasChanges = false;
        }

        public ActionEditorViewModel(
            IVariableWrapper variable,
            IEnumerable<ObjectAction> actions,
            DataProvider dataProvider)
        {
            _isInEditMode = false;
            _actions = actions;
            _objectsRepository = dataProvider.ObjectsRepository;
            _variablesRepository = dataProvider.VariablesRepository;
            _dialogsHelper = dataProvider.DialogsManager;
            _scriptRunner = new ScriptRunner(dataProvider.CommonSettings.RootPath);

            Action = new ObjectAction(variable);
            Title = "Создание действия";
            Init();
            ConditionOperand = ConditionOperands.Count == 0 ? null : ConditionOperands[0];
            Operation = ActionOperation.SetValue;
            TargetName = TargetIsEmpty ? null : TargetNames[0];
            //HasChanges = false;
        }


        public void Update()
        {
            if (Action.Condition != null)
            {
                ConditionOperand = Action.Condition.Operand;
                ConditionValue = Action.Condition.Value;
            }
            if (Action.Result != null)
            {
                Operation = Action.Result.Operation;
                TargetName = Action.Result.TargetName;
                if (ValueIsList) SelectableValue = Action.Result.Value;
                else if (ValueIsString) ResultValue = Action.Result.Value;
            }
        }


        private void InitConditionOperandsList()
        {
            ConditionOperands.Clear();
            if (Variable is StringVariableWrapper)
            {
                Enum.GetValues(typeof(ActionInputOperand))
                    .Cast<ActionInputOperand>()
                    .ToList()
                    .ForEach(t => ConditionOperands.Add(t));
            }
            else if (Variable is BoolVariableWrapper)
            {
                Enum.GetValues(typeof(ActionSelectorOperand))
                    .Cast<ActionSelectorOperand>()
                    .ToList()
                    .ForEach(t => ConditionOperands.Add(t));
            }
            else if (Variable is DateVariableWrapper)
            {
                Enum.GetValues(typeof(ActionDateOperand))
                    .Cast<ActionDateOperand>()
                    .ToList()
                    .ForEach(t => ConditionOperands.Add(t));
            }
            else if (Variable is TimeVariableWrapper)
            {
                Enum.GetValues(typeof(ActionTimeOperand))
                    .Cast<ActionTimeOperand>()
                    .ToList()
                    .ForEach(t => ConditionOperands.Add(t));
            }
        }

        private void Init()
        {
            Variable = _variablesRepository.Find(Action.Variable.Name)?.Clone();
            InitConditionOperandsList();
            HasChanges = false;
            Messenger.Default.Register<NotificationMessage<IVariableWrapper>>(this, OnVariableSelected);
            _objectsRepository.SelectionChanged += OnObjectSelected;
        }

        private void OnVariableSelected(NotificationMessage<IVariableWrapper> message)
        {
            if (message.Notification != Messages.VariableSelected) return;
            if (Operation != ActionOperation.SetValue) return;

            var selectedVariable = message.Content;
            var selector = TargetNames.FirstOrDefault(t => t == selectedVariable.Name);
            if (selector != null) TargetName = selector;
        }

        private void OnObjectSelected(IObjectViewModel selectedObject)
        {
            if (Operation != ActionOperation.SetObjectText && Operation != ActionOperation.SetVisibility) return;
            var selector = TargetNames.FirstOrDefault(t => t == selectedObject.Properties.Name);
            if (selector != null) TargetName = selector;
        }

        private async void AddVariable()
        {
            await _dialogsHelper.OpenVariableEditor(_variablesRepository, null, WindowType.ActionEditor);
            UpdateTargetNames();
        }

        private void AddCondition()
        {
            if (Action.Condition == null) Action.Condition = new ActionCondition();
            RaisePropertyChanged("HasCondition");
            if (!HasChanges) HasChanges = true;
        }

        private void RemoveCondition()
        {
            Action.Condition = null;
            RaisePropertyChanged("HasCondition");
            if (!HasChanges) HasChanges = true;
        }

        private bool IsComplete()
        {
            if (string.IsNullOrEmpty(TargetName)) return false;
            if (ValueIsList && SelectableValue == null) return false;
            if (ValueIsString && ResultValue == null) return false;
            return true;
        }

        private bool IsActionExists => _actions.Any(t => t.IsEqualTo(Action));

        private async void Save()
        {
            if (!IsComplete())
            {
                await _dialogsHelper.ShowMessage("Действие не задано.", WindowType.ActionEditor);
                return;
            }
            if (IsActionExists)
            {
                await _dialogsHelper.ShowMessage("Такое дайствие уже задано.", WindowType.ActionEditor);
                return;
            }

            IsSaved = true;
            CloseDialog();
        }

        private async void DiscardChanges()
        {
            try
            {
                if (HasChanges)
                    IsSaved = await GetRequest("Сохранить изменения?");
            }
            catch {}
            CloseDialog();
        }

        private async Task<bool> GetRequest(string text)
        {
            return await _dialogsHelper.ShowRequest(text, WindowType.ActionEditor);
        }

        private static void CloseDialog()
        {
            Messenger.Default.Send(new NotificationMessage(Messages.DoCloseActionEditor));
        }
    }
}