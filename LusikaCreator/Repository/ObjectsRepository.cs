using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using TestApp.Models;
using TestApp.Models.Config;
using TestApp.Models.Enums;
using TestApp.Models.FormObjects;
using TestApp.Models.Interfaces;
using TestApp.ViewModels.Controls;
using TestApp.ViewModels.Helpers;
using TestApp.ViewModels.Interfaces;
using TestApp.ViewModels.Variables;

namespace TestApp.Repository
{
    public class ObjectsRepository : ViewModelBase
    {
        private readonly DataProvider _dataProvider;
        private readonly VariablesRepository _variablesRepository;

        #region События

        /// <summary>
        /// Объект был выделен
        /// </summary>
        public event Handlers.ObjectChangedHandler SelectionChanged;

        /// <summary>
        /// Объект был изменен
        /// </summary>
        public event Handlers.ObjectModifiedHandler ObjectModified;

        /// <summary>
        /// Сняли выделение всех объектов
        /// </summary>
        public event Handlers.EmptyHandler Unselected;

        #endregion

        public string ErrorMessage { get; set; }

        public ObservableCollection<IObjectViewModel> ViewModels { get; set; } =
            new ObservableCollection<IObjectViewModel>();

        public ObjectsRepository(DataProvider dataProvider)
        {
            _dataProvider = dataProvider;
            _variablesRepository = dataProvider.VariablesRepository;
            _variablesRepository.VariableChanged += (variable, oldName) =>
            {
                UpdateAllFormattedText();
                var nameIsChanged = !Common.IsSameName(variable.Name, oldName);
                if (nameIsChanged) UpdateAllAssignedActions(variable, oldName);
                RefreshSelected();
            };
            _variablesRepository.VariableRemoved += variableName =>
            {
                RemoveFromAssignedObjects(ActionTargetType.Variable, variableName);
            };
            ViewModels.CollectionChanged += OnViewModelsChanged;

            DispatcherHelper.Initialize();
            Messenger.Default.Register<NotificationMessage>(this, ProcessMessage);
            Messenger.Default.Register<NotificationMessage<uint>>(this, ProcessId);
            Messenger.Default.Register<NotificationMessage<bool>>(this, ProcessBool);
        }

        public bool IsCompleted()
        {
            foreach (var vm in ViewModels.Where(t => t is IRequired))
            {
                if (!((IRequired)vm).IsComplete)
                {
                    ErrorMessage = $"{vm.Properties.Name} ({vm.Properties.Id})";
                    return false;
                }
            }
            return true;
        }

        private void OnViewModelsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                var removedObject = (IObjectViewModel) e.OldItems[0];
                RemoveFromAssignedObjects(ActionTargetType.Object, removedObject.Properties.Name);
            }
        }

        /// <summary>
        /// Удаляет все объекты или переменные из связанных действий
        /// </summary>
        private void RemoveFromAssignedObjects(ActionTargetType targetType, string targetName)
        {
            var hasChanges = false;
            foreach (var source in ViewModels.Where(t => t is IActionProperties)
                .Cast<IActionProperties>()
                .Where(t => t.ActionProperties.HasActions))
            {
                var actions = source.ActionProperties.Actions;
                for (var i = 0; i < actions.Count; i++)
                {
                    var action = actions[i];
                    if (action.TargetType != targetType) continue;

                    var targetVariableName = action.Result.TargetName;
                    if (!Common.IsSameName(targetName, targetVariableName)) continue;

                    if (!hasChanges) hasChanges = true;
                    source.ActionProperties.Actions.Remove(action);
                }
            }

            if (hasChanges && targetType == ActionTargetType.Variable) RefreshSelected();
        }

        /// <summary>
        /// При изменении переменной, обновляет все связанные с ней действия
        /// </summary>
        public void UpdateAllAssignedActions(IVariableWrapper variable, string oldName)
        {
            var hasChanges = false;
            if (variable == null || Common.IsSameName(variable.Name, oldName)) return;

            foreach (var vm in ViewModels.Where(t =>
                t is IActionProperties && ((IActionProperties) t).ActionProperties.HasActions))
            {
                var objectActions = ((IActionProperties) vm).ActionProperties.Actions;
                foreach (var action in objectActions.Where(t => t.TargetType == ActionTargetType.Variable))
                {
                    if (!Common.IsSameName(action.Result.TargetName, oldName)) continue;
                    action.Result.TargetName = variable.Name;
                    if (!hasChanges) hasChanges = true;
                }
            }

            if (hasChanges) RefreshSelected();
        }

        /// <summary>
        /// При изменении имени объекта, обновляет все связанные с ним действия
        /// </summary>
        public void UpdateAllAssignedActions(string oldObjectName, string newObjectName)
        {
            var hasChanges = false;
            foreach (var viewModel in ViewModels.Where(t => t is IActionProperties))
            {
                var vm = (IActionProperties) viewModel;
                if (!vm.ActionProperties.HasActions) continue;

                foreach (var action in vm.ActionProperties.Actions
                    .Where(t => t.TargetType == ActionTargetType.Object)
                    .Where(action => Common.IsSameName(action.Result.TargetName, oldObjectName)))
                {
                    action.Result.TargetName = newObjectName;
                    if (!hasChanges) hasChanges = true;
                }
            }

            if (hasChanges) RefreshSelected();
        }


        private void RefreshSelected()
        {
            var selected = ViewModels.FirstOrDefault(t => t.Properties.IsSelected);
            if (selected != null) SelectionChanged?.Invoke(selected);
        }

        private void ProcessMessage(NotificationMessage message)
        {
            switch (message.Notification)
            {
                case Messages.DoUnselectAll:
                    Unselected?.Invoke();
                    break;
            }
        }

        private void ProcessId(NotificationMessage<uint> message)
        {
            switch (message.Notification)
            {
                case Messages.DoSelectObject:
                    Select(message.Content);
                    break;
            }
        }

        private void ProcessBool(NotificationMessage<bool> message)
        {
            switch (message.Notification)
            {
                case Messages.DoSwitchAppMode:
                    var isDebugMode = message.Content;
                    _dataProvider.CommonSettings.AppMode = isDebugMode ? AppMode.Debug : AppMode.Editor;
                    break;
            }
        }

        private IVariableWrapper CreateVariable(VariableType type)
        {
            return _variablesRepository.Add(type, isAssigned: true);
        }

        public IObjectViewModel Add(ObjectType objectType)
        {
            ErrorMessage = string.Empty;

            var id = MakeId();
            var name = $"Объект {id}";

            IObjectViewModel viewModel;
            IVariableWrapper variable;
            switch (objectType)
            {
                case ObjectType.Label:
                    viewModel = new LabelViewModel(id, name, _dataProvider);
                    break;

                case ObjectType.TextBox:
                    variable = CreateVariable(VariableType.String);
                    viewModel = new TextBoxViewModel(id, name, variable, _dataProvider);
                    break;

                case ObjectType.CheckBox:
                    variable = CreateVariable(VariableType.Bool);
                    viewModel = new CheckBoxViewModel(id, name, variable, _dataProvider);
                    break;

                case ObjectType.RadioButton:
                    variable = CreateVariable(VariableType.Bool);
                    viewModel = new RadioViewModel(id, name, variable, _dataProvider)
                    {
                        GroupName = GetRadioGroupName()
                    };
                    break;

                case ObjectType.DatePicker:
                    variable = CreateVariable(VariableType.Date);
                    viewModel = new DatePickerViewModel(id, name, variable, _dataProvider);
                    break;

                case ObjectType.TimePicker:
                    variable = CreateVariable(VariableType.Time);
                    viewModel = new TimePickerViewModel(id, name, variable, _dataProvider);
                    break;

                default:
                    ErrorMessage = "Неизвестный тип объекта.";
                    return null;
            }

            viewModel.Properties.TabId = _dataProvider.TabsRepository.SelectedTabIndex;
            viewModel.Properties.Left = 20;
            viewModel.Properties.Top = 20;
            viewModel.Properties.FontSettings = new FontSettings();
            ViewModels.Add(viewModel);
            Select(viewModel);
            ObjectModified?.Invoke(viewModel.Properties.Id);

            return viewModel;
        }

        /// <summary>
        /// Добавление десериализуемого объекта
        /// </summary>
        public void Add(IFormObject formObject)
        {
            IObjectViewModel viewModel = null;
            if (formObject is CheckBoxObject)
            {
                viewModel = new CheckBoxViewModel((CheckBoxObject)formObject, _dataProvider);
            }
            else if (formObject is LabelObject)
            {
                viewModel = new LabelViewModel((LabelObject)formObject, _dataProvider);
            }
            else if (formObject is TextBoxObject)
            {
                viewModel = new TextBoxViewModel((TextBoxObject)formObject, _dataProvider);
            }
            else if (formObject is RadioButtonObject)
            {
                viewModel = new RadioViewModel((RadioButtonObject)formObject, _dataProvider);
            }
            else if (formObject is DateBoxObject)
            {
                viewModel = new DatePickerViewModel((DateBoxObject)formObject, _dataProvider);
            }
            else if (formObject is TimePickerObject)
            {
                viewModel = new TimePickerViewModel((TimePickerObject)formObject, _dataProvider);
            }

            if (viewModel == null) return;

            viewModel.Properties.Left = formObject.Left;
            viewModel.Properties.Top = formObject.Top;
            viewModel.Properties.IsSelected = false;
            viewModel.Properties.SetVisibility(formObject.IsVisible);
            viewModel.Properties.TabId = formObject.TabId;
            ViewModels.Add(viewModel);
            ObjectModified?.Invoke(viewModel.Properties.Id);
        }

        private void SelectRadioGroup(string groupName)
        {
            foreach (var viewModel in ViewModels.Where(t => t is RadioViewModel)
                .Cast<RadioViewModel>()
                .Where(viewModel => viewModel.GroupName == groupName))
            {
                viewModel.Properties.Highlight();
            }
        }

        private uint MakeId()
        {
            if (ViewModels.Count == 0) return 1;
            var lastId = ViewModels.OrderBy(t => t.Properties.Id).Last().Properties.Id;
            return lastId + 1;
        }

        /// <summary>
        /// Проверяет наличие группы переключателей с заданным именем
        /// </summary>
        public bool IsRadioGroupExists(string groupName)
        {
            var groupNames = GetGroupNames();
            if (groupNames.Count == 0) return false;

            return groupNames.FirstOrDefault(t => Common.IsSameName(t, groupName)) != null;
        }

        private string GetRadioGroupName()
        {
            const string prefix = "Группа";
            var id = GetGroupNames().Count;
            string groupName;
            do
            {
                groupName = $"{prefix}_{++id}";
            } while (IsRadioGroupExists(groupName));

            return groupName;
        }

        private List<string> GetGroupNames()
        {
            var ret = new List<string>();

            foreach (var radio in ViewModels.Where(t =>
                t.GetType() == typeof (RadioViewModel)).Cast<RadioViewModel>())
            {
                var name = radio.GroupName;
                if (!ret.Contains(name)) ret.Add(name);
            }
            return ret;
        }

        public List<string> GetNames()
        {
            return ViewModels
                .Where(t => !string.IsNullOrEmpty(t.Properties.Name))
                .Select(t => t.Properties.Name).ToList();
        }

        /// <summary>
        /// Возвращает имена объектов формы, у которых есть текст
        /// </summary>
        public List<string> GetNamesOfObjectsWithText()
        {
            return (from viewModel in ViewModels where !string.IsNullOrEmpty(viewModel.Properties.Name)
                select viewModel.Properties.Name).ToList();
        }

        public bool Remove(uint objectId)
        {
            var selector = Find(objectId);
            if (selector == null)
            {
                ErrorMessage = $"Объект с идентификатором ${objectId} не найден.";
                return false;
            }

            if (selector is IActionProperties)
            {
                // Удаление связанной переменной
                var variable = ((IActionProperties) selector).ActionProperties.Variable;
                _variablesRepository.Remove(variable.Name);
            }

            ViewModels.Remove(selector);

            return true;
        }

        /// <summary>
        /// Возвращает имя объекта, которому принадлежит переменная
        /// </summary>
        public IObjectViewModel GetAssignedObject(string variableName)
        {
            return ViewModels.FirstOrDefault(obj => obj is IActionProperties && Common.IsSameName(((IActionProperties) obj).ActionProperties.Variable.Name, variableName));
        }

        private static List<ObjectAction> GetActions(IActionProperties selector)
            => selector?.ActionProperties.Actions;

        public bool ProcessActions(IObjectViewModel viewModel)
        {
            var actions = GetActions((IActionProperties) viewModel);
            if (actions == null)
            {
                ErrorMessage = $"Ошибка запроса действий у объекта \"{viewModel.Properties.Name}\"";
                return false;
            }

            var executableActions = actions
                .Where(action => !action.HasCondition || ConditionIsTrue(action, viewModel))
                .ToList();
            if (executableActions.Count > 0)
            {
                foreach (var action in executableActions) ApplyActionResult(action.Result);
            }

            _variablesRepository.UpdateAllFormattedValues();
            UpdateAllFormattedText();
            _dataProvider.ProjectRepository.ProjectSettings.UpdateTitle();
            return true;
        }

        /// <summary>
        /// Заменяет измененный тег у текстовых полей всех объектов, содержащих этот тег
        /// </summary>
        /// <param name="sourceVariable"></param>
        public void UpdateAllFormattedText(IVariableWrapper sourceVariable)
        {
            var tagName = "{#" + sourceVariable.Name + "}";
            var replaceTo = sourceVariable.StringValue;

            foreach (var viewModel in ViewModels
                .Where(t => t is ITextProperties)
                .Cast<ITextProperties>()
                .Where(obj => obj.TextProperties.Text.Contains(tagName)))
            {
                viewModel.TextProperties.FormattedText = viewModel.TextProperties.Text.Replace(tagName, replaceTo);
            }
        }


        public void UpdateAllFormattedText()
        {
            foreach (var viewModel in ViewModels.Where(t => t is ITextProperties))
            {
                ((ITextProperties) viewModel).TextProperties.UpdateFormattedText();
            }
        }


        private bool ConditionIsTrue(ObjectAction action, IObjectViewModel selector)
        {
            if (action.Condition.IsVariableTarget)
            {
                var variableName = ((IActionProperties) selector).ActionProperties.Variable.Name;
                var targetVariable = _variablesRepository.Find(variableName);
                if (targetVariable == null)
                {
                    // todo error
                    return false;
                }
                return ConditionIsTrue(targetVariable, action.Condition);
            }
            return ConditionIsTrue(selector, action.Condition);
        }

        private static string WrongFormatMessage(IVariableWrapper variable = null)
        {
            return variable == null
                ? "Несоответствие условия заданномму типу."
                : $"Тип значения переменной \"{variable.Name}\" не соответствует типу условия ({variable.GetType()}).";
        }


        private const string UnknownOperationMessage = "Неизвестный тип операции";

        private bool ConditionIsTrue(IVariableWrapper variable, ActionCondition condition)
        {
            if (condition == null) return false;
            var type = condition.Operand.GetType();

            #region Сравнение со строкой

            if (type == typeof (ActionInputOperand))
            {
                if (!(variable is StringVariableWrapper) || !(condition.Value is string))
                {
                    ErrorMessage = WrongFormatMessage(variable);
                    return false;
                }

                var operand = (ActionInputOperand) condition.Operand;
                var value = ((StringVariableWrapper) variable).Value.Trim().ToLower();
                var comparedValue = ((string) condition.Value).Trim().ToLower();

                switch (operand)
                {
                    case ActionInputOperand.Set:
                        return !string.IsNullOrEmpty(value);
                    case ActionInputOperand.NotSet:
                        return string.IsNullOrEmpty(value);
                    case ActionInputOperand.Equal:
                        return string.Equals(value, comparedValue);
                    case ActionInputOperand.NotEqual:
                        return !string.Equals(value, comparedValue);
                    case ActionInputOperand.Contains:
                        return value.Contains(comparedValue);
                }
                ErrorMessage = UnknownOperationMessage;
                return false;
            }

            #endregion

            #region Сравнение с логическим значением

            if (type == typeof (ActionSelectorOperand))
            {
                if (!(variable is BoolVariableWrapper))
                {
                    ErrorMessage = WrongFormatMessage(variable);
                    return false;
                }

                var boolVariable = (BoolVariableWrapper) variable;
                var operand = (ActionSelectorOperand) condition.Operand;

                switch (operand)
                {
                    case ActionSelectorOperand.Set:
                        return boolVariable.IsSet;
                    case ActionSelectorOperand.NotSet:
                        return !boolVariable.IsSet;
                }
                ErrorMessage = UnknownOperationMessage;
                return false;
            }

            #endregion

            #region Сравнение с датой

            if (type == typeof (ActionDateOperand))
            {
                if (!(variable is DateVariableWrapper))
                {
                    ErrorMessage = WrongFormatMessage(variable);
                    return false;
                }

                var dateVariable = (DateVariableWrapper) variable;
                var operand = (ActionDateOperand) condition.Operand;

                switch (operand)
                {

                }

                ErrorMessage = UnknownOperationMessage;
                return false;
            }

            #endregion

            #region Сравнение со временем

            if (type == typeof (ActionDateOperand))
            {
                if (!(variable is TimeVariableWrapper))
                {
                    ErrorMessage = WrongFormatMessage(variable);
                    return false;
                }
            }

            #endregion

            ErrorMessage = UnknownOperationMessage;
            return false;
        }

        private bool ConditionIsTrue(IObjectViewModel viewModel, ActionCondition condition)
        {
            if (condition == null) return false;
            var type = condition.Operand.GetType();

            #region Сравнение видимости

            if (type == typeof (ActionVisibilityOperand))
            {
                if (!(condition.Value is ActionVisibilityOperand))
                {
                    ErrorMessage = WrongFormatMessage();
                    return false;
                }

                var operand = (ActionVisibilityOperand) condition.Operand;
                var value = viewModel.Properties.GetVisibility();

                switch (operand)
                {
                    case ActionVisibilityOperand.Hide:
                        return !value;
                    case ActionVisibilityOperand.Show:
                        return value;
                }
            }

            #endregion

            ErrorMessage = UnknownOperationMessage;
            return false;
        }

        /// <summary>
        /// Заменяет в строке содержимое тегов {#...} на значения переменных при их наличии
        /// </summary>
        public string ReplaceValuesInString(string val)
        {
            var list = ExtractTagsFromString(val);

            var ret = val;
            foreach (var item in list)
            {
                var variable = _variablesRepository.Find(item);
                if (variable == null) continue;

                var replacedText = "{#" + item + "}";
                ret = val.Replace(replacedText, variable.ToString());
            }

            return ret;
        }

        /// <summary>
        /// Возвращает список имен тегов, содержащихся в строке без "{#" и "}"
        /// </summary>
        public List<string> ExtractTagsFromString(string val)
        {
            var ret = new List<string>();
            if (string.IsNullOrEmpty(val)) return ret;
            var from = 0;

            int startIndex;
            do
            {
                startIndex = val.IndexOf("{#", from, StringComparison.Ordinal);
                if (startIndex != -1)
                {
                    from = startIndex + 2;
                    var endIndex = val.IndexOf("}", @from, StringComparison.Ordinal);

                    if (endIndex != -1)
                    {
                        ret.Add(val.Substring(from, endIndex - from));
                        from = endIndex + 1;
                    }
                }
            } while (startIndex != -1);

            return ret;
        }

        public string GetObjectText(IObjectViewModel viewModel, bool isFormattedText = false)
        {
            if (!(viewModel is ITextProperties)) return null;

            var vm = (ITextProperties) viewModel;
            return isFormattedText ? vm.TextProperties.FormattedText : vm.TextProperties.Text;
        }

        public string GetObjectText(string objectName, bool isFormattedText = false)
        {
            var viewModel = Find(objectName);
            return viewModel == null ? null : GetObjectText(viewModel);
        }

        private bool ApplyActionResult(ActionResult result)
        {
            #region Изменение переменной

            if (result.Operation == ActionOperation.SetValue)
            {
                var targetVariable = _variablesRepository.Find(result.TargetName);
                if (targetVariable == null)
                {
                    ErrorMessage = $"Переменная с именем \"{result.TargetName}\" не найдена.";
                    return false;
                }

                if (targetVariable is StringVariableWrapper)
                {
                    if (!(result.Value is string)) return false;
                    ((StringVariableWrapper) targetVariable).Value = (string) result.Value;
                }
                else if (targetVariable is BoolVariableWrapper)
                {
                    if (!(result.Value is ActionSelectorOperand)) return false;
                    ((BoolVariableWrapper) targetVariable).Value = (ActionSelectorOperand) result.Value;
                }
                else if (targetVariable is DateVariableWrapper)
                {
                    if (!(result.Value is DateTime)) return false;
                    ((DateVariableWrapper) targetVariable).Value = (DateTime) result.Value;
                }
                else if (targetVariable is TimeVariableWrapper)
                {
                    if (!(result.Value is TimeSpan)) return false;
                    ((TimeVariableWrapper) targetVariable).Value = (TimeSpan) result.Value;
                }
                //_variablesRepository.UpdateTags(targetVariable);
                return true;
            }

            #endregion

            #region Изменение объекта

            var targetObject = Find(result.TargetName);
            if (targetObject == null)
            {
                ErrorMessage = $"Объект с именем \"{result.TargetName}\" не найден.";
                return false;
            }

            switch (result.Operation)
            {
                case ActionOperation.SetObjectText:
                    if (!(result.Value is string))
                    {
                        ErrorMessage = $"Несоответствие типов условия для переменной \"{result.TargetName}\"";
                        return false;
                    }

                    if (targetObject is ITextProperties)
                        ((ITextProperties) targetObject).TextProperties.Text = (string) result.Value;
                    break;

                case ActionOperation.SetVisibility:
                    if (!(result.Value is ActionVisibilityOperand))
                    {
                        ErrorMessage = $"Несоответствие типов условия для переменной \"{result.TargetName}\"";
                        return false;
                    }

                    var isVisible = (ActionVisibilityOperand) result.Value == ActionVisibilityOperand.Show;
                    targetObject.Properties.SetVisibility(isVisible);
                    break;
            }

            #endregion

            return true;
        }

        public void Select(uint objectId)
        {
            var viewModel = Find(objectId);
            if (viewModel == null) return;

            Select(viewModel);
        }

        public void Select(IObjectViewModel viewModel)
        {
            if (viewModel?.Properties == null ||
                _dataProvider.CommonSettings.AppMode != AppMode.Editor) return;

            var type = viewModel.GetType();
            UnselectAll();

            if (type == typeof (RadioViewModel))
            {
                SelectRadioGroup(((RadioViewModel) viewModel).GroupName);
            }

            viewModel.Properties.IsSelected = true;
            SelectionChanged?.Invoke(viewModel);
        }

        public void SwitchAppMode()
        {
            foreach (var viewModel in ViewModels)
            {
                viewModel.Properties.SwitchAppMode(_dataProvider.CommonSettings.AppMode);
                if (viewModel is IRequired)
                {
                    ((IRequired) viewModel).Refresh();
                }
            }
        }

        public IObjectViewModel Find(uint objectId)
        {
            return ViewModels.FirstOrDefault(t => t.Properties.Id == objectId);
        }

        public IObjectViewModel Find(string objectName)
        {
            return ViewModels
                .Where(t => !string.IsNullOrEmpty(t.Properties.Name))
                .FirstOrDefault(t => Common.IsSameName(t.Properties.Name, objectName));
        }

        public IObjectViewModel FindByVariableName(string variableName)
        {
            return ViewModels
                .Where(t => t is IActionProperties)
                .FirstOrDefault(t => ((IActionProperties) t).ActionProperties.Variable != null &&
                                     Common.IsSameName(((IActionProperties) t).ActionProperties.Variable.Name, variableName));
        }

        public bool Update(IObjectViewModel buffer)
        {
            var viewModel = Find(buffer.Properties.Id);
            if (viewModel == null) return true;

            var nameIsChanged = !Common.IsSameName(buffer.Properties.Name, viewModel.Properties.Name);
            if (nameIsChanged)
            {
                if (NameExists(buffer.Properties.Name))
                {
                    ErrorMessage = $"Объект с именем \"{buffer.Properties.Name}\" уже существует.";
                    return false;
                }

                UpdateAllAssignedActions(viewModel.Properties.Name, buffer.Properties.Name);
            }

            if (viewModel is RadioViewModel)
            {
                SelectOnlyOneRadioInGroup((RadioViewModel) buffer);
            }

            if (viewModel is IActionProperties && buffer is IActionProperties)
            {
                var vm = (IActionProperties) viewModel;
                var buf = (IActionProperties) buffer;

                var variableIsChanged = vm.ActionProperties.Variable != null &&
                                        !vm.ActionProperties.Variable.IsEqualTo(buf.ActionProperties.Variable);
                if (variableIsChanged)
                {
                    var oldVariableName =
                        !Common.IsSameName(buf.ActionProperties.Variable.Name, vm.ActionProperties.Variable.Name)
                            ? vm.ActionProperties.Variable.Name
                            : null;
                    if (!_variablesRepository.AddOrUpdate(buf.ActionProperties.Variable, oldVariableName))
                    {
                        ErrorMessage = _variablesRepository.ErrorMessage;
                        return false;
                    }
                }
            }

            viewModel.Update(buffer);
            Select(viewModel);
            ObjectModified?.Invoke(viewModel.Properties.Id);
            return true;
        }

        private void SelectOnlyOneRadioInGroup(RadioViewModel viewModel)
        {
            var groupName = viewModel.GroupName;
            var relatedRadios = ViewModels
                .Where(t => t is RadioViewModel && Common.IsSameName(groupName, ((RadioViewModel) t).GroupName))
                .Cast<IActionProperties>()
                .ToList();

            var isAnySelected = relatedRadios.Any(t => ((BoolVariableWrapper) t.ActionProperties.Variable).IsSet);
            if (!isAnySelected)
            {
                ((BoolVariableWrapper) viewModel.ActionProperties.Variable).Set(true);
                return;
            }

            if (((BoolVariableWrapper) viewModel.ActionProperties.Variable).IsSet)
            {
                foreach (var radio in relatedRadios)
                {
                    ((BoolVariableWrapper) radio.ActionProperties.Variable).Set(false);
                }
            }
        }

        public bool NameExists(string objectName) => Find(objectName) != null;

        public void Clear()
        {
            ViewModels.Clear();
        }

        public void UnselectAll()
        {
            foreach (var viewModel in ViewModels)
            {
                viewModel.Properties.Unselect();
            }
        }
    }
}