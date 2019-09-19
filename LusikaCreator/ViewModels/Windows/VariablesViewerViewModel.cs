using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using TestApp.Models;
using TestApp.Models.Config;
using TestApp.Models.Enums;
using TestApp.Repository;
using TestApp.ViewModels.Helpers;
using TestApp.ViewModels.Interfaces;

namespace TestApp.ViewModels.Windows
{
    public class VariablesViewerViewModel : ViewModelBase
    {
        private readonly VariablesRepository _variablesRepository;
        private readonly ObjectsRepository _objectsRepository;
        private readonly DialogsManager _dialogsHelper;

        public RelayCommand AddCommand => new RelayCommand(Add);
        public RelayCommand EditCommand => new RelayCommand(Edit);
        public RelayCommand RemoveCommand => new RelayCommand(Remove);
        public RelayCommand RemoveFilterCommand => new RelayCommand(RemoveFilter);
        public RelayCommand<string> CopyCommand => new RelayCommand<string>(CopyToClipboard);

        public CollectionViewSource FilteredVariables { get; set; } = new CollectionViewSource();

        public WindowSettings Settings { get; set; }

        public bool IsInEditMode
        {
            get { return _isInEditMode; }
            set
            {
                Set(ref _isInEditMode, value);
                if (!value) RemoveBtnIsEnabled = false;
            }
        }
        private bool _isInEditMode;

        public IVariableWrapper Selected
        {
            get { return _selected; }
            set
            {
                Set(ref _selected, value);
                if (!IsInEditMode) RemoveBtnIsEnabled = false;
                else RemoveBtnIsEnabled = value != null;
                Messenger.Default.Send(new NotificationMessage<IVariableWrapper>(value, Messages.VariableSelected));
            }
        }
        private IVariableWrapper _selected;

        public bool RemoveBtnIsEnabled
        {
            get { return _removeBtnIsEnabled; }
            set { Set(ref _removeBtnIsEnabled, value); }
        }
        private bool _removeBtnIsEnabled;

        public List<VariableType> VariableTypes => Enum.GetValues(typeof (VariableType)).Cast<VariableType>()
            .Where(t => t != VariableType.Unknown).ToList();

        public VariableType FilteredType
        {
            get { return _filteredType; }
            set
            {
                Set(ref _filteredType, value);
                UpdateFilter();
            }
        }
        private VariableType _filteredType = VariableType.Any;

        public string FilteredName
        {
            get { return _filteredName; }
            set
            {
                Set(ref _filteredName, value);
                UpdateFilter();
            }
        }
        private string _filteredName = string.Empty;

        public string FilteredCount
        {
            get { return _filteredCount; }
            set { Set(ref _filteredCount, value); }
        }
        private string _filteredCount;

        public string Title
        {
            get { return _title; }
            set { Set(ref _title, value); }
        }
        private string _title;

        public VariablesViewerViewModel(DataProvider dataProvider)
        {
            Settings = dataProvider.CommonSettings.VariablesViewerSettings;
            _variablesRepository = dataProvider.VariablesRepository;
            _objectsRepository = dataProvider.ObjectsRepository;
            _dialogsHelper = dataProvider.DialogsManager;
            FilteredVariables.Source = _variablesRepository.Variables;
            FilteredVariables.View.CollectionChanged += OnFilteredChanged;
            _variablesRepository.Variables.CollectionChanged += delegate { UpdateTitle(); };
            Title = "Переменные";
            dataProvider.CommonSettings.AppModeChanged += delegate
            {
                IsInEditMode = dataProvider.CommonSettings.AppMode == AppMode.Editor;
            };
        }

        private void CopyToClipboard(string param)
        {
            if (Selected == null) return;

            string text = null;
            if (param == "name") text = Selected.Name;
            else if (param == "value") text = Selected.StringValue;
            if (text == null) return;

            Clipboard.SetText(text);
            _dialogsHelper.ShowNotification("Значение скопировано в буфер обмена");
        }

        private void UpdateTitle(int? filteredCount = null)
        {
            var totalCount = _variablesRepository.Variables.Count;
            if (totalCount == 0)
            {
                Title = "Переменные";
                return;
            }

            if (filteredCount.HasValue)
            {
                var cnt = filteredCount.Value;
                Title = $"Переменые ({(cnt == totalCount ? $"{totalCount}" : $"{cnt}/{totalCount}")})";
            }
            else Title = $"Переменые ({totalCount})";
        }

        private void OnFilteredChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (FilterIsEmpty)
            {
                FilteredCount = string.Empty;
                return;
            }
            var cnt = FilteredVariables.View.Cast<IVariableWrapper>().Count();
            FilteredCount = cnt == 0 ? "Совпадений не найдено" : $"Всего совпадений: {cnt}";
            UpdateTitle(cnt);
        }

        private void UpdateFilter()
        {
            FilteredVariables.Filter -= Filter;
            FilteredVariables.Filter += Filter;
        }

        private void RemoveFilter()
        {
            FilteredType = VariableType.Any;
            FilteredName = string.Empty;
            FilteredVariables.Filter -= Filter;
            UpdateTitle();
        }

        private bool FilterIsEmpty => string.IsNullOrEmpty(FilteredName) && FilteredType == VariableType.Any;

        private void Filter(object sender, FilterEventArgs e)
        {
            var variable = (IVariableWrapper) e.Item;
            var currentName = variable.Name.Trim().ToLower();
            var currentType = variable.Type;

            if (FilteredType != VariableType.Any)
            {
                if (currentType != FilteredType)
                {
                    e.Accepted = false;
                    return;
                }
            }

            if (!string.IsNullOrEmpty(FilteredName))
            {
                if (!currentName.Contains(FilteredName))
                {
                    e.Accepted = false;
                    return;
                }
            }

            e.Accepted = true;
        }


        private async void Add()
        {
            await _dialogsHelper.OpenVariableEditor(_variablesRepository, null, WindowType.VariablesViewer);
        }

        private async void Remove()
        {
            if (Selected == null) return;

            var assignedObject = _objectsRepository.GetAssignedObject(Selected.Name);
            if (assignedObject != null)
            {
                await _dialogsHelper.ShowMessage($"Переменная \"{Selected.Name}\" связана с объектом {assignedObject.Properties.Id} (\"" +
                                                $"{assignedObject.Properties.Name}\"). Удалите этот объект.",
                    WindowType.VariablesViewer);
                return;
            }

            if (await _dialogsHelper.ShowRequest($"Удалить переменную \"{Selected.Name}\"?", WindowType.VariablesViewer))
            {
                _variablesRepository.Remove(Selected.Name);
            }
        }

        private async void Edit()
        {
            await _dialogsHelper.OpenVariableEditor(_variablesRepository, Selected, WindowType.VariablesViewer);
        }

    }
}