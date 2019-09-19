using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Data;
using GalaSoft.MvvmLight;
using TestApp.Models;
using TestApp.Models.Enums;
using TestApp.Models.Helpers;
using TestApp.Models.Interfaces;
using TestApp.Models.Variables;
using TestApp.ViewModels.Interfaces;
using TestApp.ViewModels.Variables;

namespace TestApp.Repository
{
    public class VariablesRepository : ViewModelBase
    {
        private readonly List<IVariableWrapper> _savedStates = new List<IVariableWrapper>();

        public ObservableCollection<IVariableWrapper> Variables;

        public CollectionViewSource FilteredVariables
        {
            get { return _filteredVariables; }
            set { Set(ref _filteredVariables, value); }
        }
        private CollectionViewSource _filteredVariables = new CollectionViewSource();

        public event Handlers.VariableChangedHandler VariableChanged;
        public event Handlers.VariableRemovedHandler VariableRemoved;

        public string ErrorMessage { get; set; } = string.Empty;

        public string EmptyNameMessage => "Не задано имя переменной.";

        public string VariableExistsMessage(string variableName)
            => $"Переменная с именем \"{variableName}\"уже существует." +
               $"{Environment.NewLine}Задайте другое имя.";

        public VariablesRepository(DataProvider dataProvider)
        {
            Variables = new ObservableCollection<IVariableWrapper>();
            Variables.CollectionChanged += OnVariablesChanged;
            FilteredVariables.Source = Variables;
            dataProvider.CommonSettings.AppModeChanged +=
                delegate { OnAppModeChanged(dataProvider.CommonSettings.AppMode); };
        }

        private void OnAppModeChanged(AppMode mode)
        {
            if (mode == AppMode.Debug)
            {
                _savedStates.Clear();
                foreach (var variable in Variables.Where(t => t.IsAssigned))
                {
                    _savedStates.Add(variable.Clone());
                }
            }
            else if (mode == AppMode.Editor)
            {
                if (_savedStates.Count == 0) return;

                foreach (var variable in _savedStates)
                {
                    var selector = Variables.FirstOrDefault(t => Common.IsSameName(t.Name, variable.Name));
                    if (selector != null && selector.StringValue != variable.StringValue)
                    {
                        selector.Update(variable);
                    }
                }
                _savedStates.Clear();
            }
        }

        private void OnVariablesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                var removedVariable = (IVariableWrapper)e.OldItems[0];
                VariableRemoved?.Invoke(removedVariable.Name);
            }
        }

        public IVariableWrapper Find(string variableName)
        {
            if (!IsInitialized()) return null;
            if (string.IsNullOrEmpty(variableName)) return null;
            if (Variables.Count == 0) return null;

            return Variables.FirstOrDefault(t => Common.IsSameName(t.Name, variableName));
        }

        public bool IsExists(string variableName) => Find(variableName) != null;

        public IVariableWrapper Add(VariableType type = VariableType.String, object value = null, bool isAssigned = false)
        {
            if (!IsInitialized()) return null;

            IVariableWrapper wrapper = null;

            var name = MakeName();
            switch (type)
            {
                case VariableType.String:
                    wrapper = new StringVariableWrapper(name, this, isAssigned);
                    break;

                case VariableType.Bool:
                    wrapper = new BoolVariableWrapper(name, isAssigned);
                    break;

                case VariableType.Date:
                    wrapper = new DateVariableWrapper(name, isAssigned);
                    break;

                case VariableType.Time:
                    wrapper = new TimeVariableWrapper(name, isAssigned);
                    break;
            }

            if (wrapper == null)
            {
                ErrorMessage = $"Запрос на создание переменной неизвестного типа \"{type}\"";
                return null;
            }

            Variables.Add(wrapper);
            if (value != null) wrapper.Set(value);

            return wrapper;
        }

        /// <summary>
        /// Добавление десериализуемого объекта
        /// </summary>
        public void Add(IVariable storedVariable)
        {
            if (storedVariable is StringVariable)
            {
                var variable = (StringVariable) storedVariable;
                var wrapper = new StringVariableWrapper(variable.Name, this) {Value = variable.Value};
                Variables.Add(wrapper);
            }
            else if (storedVariable is BoolVariable)
            {
                var variable = (BoolVariable) storedVariable;
                var wrapper = new BoolVariableWrapper(variable.Name) {Value = variable.Value};
                Variables.Add(wrapper);
            }
            else if (storedVariable is DateVariable)
            {
                var variable = (DateVariable) storedVariable;
                var wrapper = new DateVariableWrapper(variable.Name)
                {
                    UseCurrentDate = variable.UseCurrentDate,
                    Format = variable.Format,
                    Value = variable.UseCurrentDate ? DateTime.Today : variable.Value
                };
                Variables.Add(wrapper);
            }
            else if (storedVariable is TimeVariable)
            {
                var variable = (TimeVariable) storedVariable;
                var wrapper = new TimeVariableWrapper(variable.Name)
                {
                    UseCurrentTime = variable.UseCurrentTime,
                    UseSeconds = variable.UseSeconds,
                    Format = variable.Format,
                    Value = variable.UseCurrentTime ? DateTime.Now.TimeOfDay : variable.Value
                };
                Variables.Add(wrapper);
            }
        }

        public bool AddOrUpdate(IVariableWrapper variable, string oldName = null)
        {
            if (!IsInitialized()) return false;

            if (string.IsNullOrEmpty(variable.Name))
            {
                ErrorMessage = EmptyNameMessage;
                return false;
            }

            var nameIsChanged = !Common.IsSameName(oldName, variable.Name);
            IVariableWrapper selector;
            if (!string.IsNullOrEmpty(oldName))
            {
                // Изменилось имя переменной
                if (nameIsChanged && IsExists(variable.Name))
                {
                    // Переименовали существующую переменную
                    ErrorMessage = VariableExistsMessage(variable.Name);
                    return false;
                }
                selector = Find(oldName);
            }
            else selector = Find(variable.Name);

            if (selector == null) Variables.Add(variable);
            else
            {
                selector.Update(variable);
                VariableChanged?.Invoke(variable, oldName);
                UpdateTags(variable);
            }

            return true;
        }

        public void UpdateAllFormattedValues()
        {
            Variables.ToList().ForEach(UpdateTags);
        }

        /// <summary>
        /// Обновляет форматированный текст у всех переменных, содержащих этот тег
        /// </summary>
        public void UpdateTags(IVariableWrapper sourceVariable)
        {
            foreach (var variable in Variables
                .Where(t => t is StringVariableWrapper)
                .Cast<StringVariableWrapper>()
                .Where(variable => variable.IsContainTag(sourceVariable)))
            {
                variable.UpdateFormattedValue();// .UpdateTag(sourceVariable);
            }
        }


        public void Clear()
        {
            Variables.Clear();
        }

        public List<string> GetNames(string skippedName = null)
        {
            var ret = new List<string>();
            foreach (var name in Variables.Select(t => t.Name)
                .Where(name => skippedName == null || !string.Equals(name.Trim(), skippedName.Trim(),
                    StringComparison.CurrentCultureIgnoreCase))
                .Where(name => !ret.Contains(name.Trim().ToLower())))
            {
                ret.Add(name);
            }
            return ret;
        }

        public bool Remove(string name)
        {
            if (!IsInitialized()) return false;

            var variable = Find(name);
            if (variable == null)
            {
                ErrorMessage = VariableExistsMessage(name);
                return false;
            }

            Variables.Remove(variable);
            return true;
        }

        private bool IsInitialized()
        {
            if (Variables != null) return true;
            ErrorMessage = "Список переменных не инициализирован.";
            return false;
        }

        public string MakeName()
        {
            var id = 0;
            string name;

            do
            {
                name = $"Переменная {++id}";
            } while (IsExists(name));

            return name;
        }

        public string GetFormattedText(string text)
        {
            var ret = new TagReplacer(this).GetFormattedString(text) ?? text;
            return ret;
        }
    }
}