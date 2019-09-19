using System;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using TestApp.Models;
using TestApp.Repository;
using TestApp.ViewModels.Helpers;
using TestApp.ViewModels.Interfaces;

namespace TestApp.ViewModels.Dialogs
{
    public class VariableEditorViewModel : ViewModelBase
    {
        private readonly VariablesRepository _variablesRepository;
        private readonly DialogsManager _dialogsHelper;
        private readonly bool _isCreateMode;

        public string Title
        {
            get { return _title; }
            set { Set(ref _title, value); }
        }
        private string _title;

        public ValuesSwitcherViewModel Switcher { get; set; }

        public RelayCommand CloseCommand => new RelayCommand(Close);
        public RelayCommand ApplyChangesCommand => new RelayCommand(SaveChanges);
        public RelayCommand DiscardChangesCommand => new RelayCommand(DiscardChanges);


        public VariableEditorViewModel(DataProvider dataProvider, IVariableWrapper variable = null)
        {
            _variablesRepository = dataProvider.VariablesRepository;
            _dialogsHelper = dataProvider.DialogsManager;
            _isCreateMode = variable == null;
            Title = $"{(variable == null ? "Создание" : "Редактирование")} переменной";
            Switcher = new ValuesSwitcherViewModel(dataProvider, variable, _isCreateMode);
        }

        private static void Close()
        {
            Messenger.Default.Send(new NotificationMessage(Messages.CloseVariableEditor));
        }

        private async void SaveChanges()
        {
            if (_isCreateMode && _variablesRepository.IsExists(Switcher.Variable.Name))
            {
                await _dialogsHelper.ShowMessage(
                    $"Переменная с именем \"{Switcher.Variable.Name}\" уже существует.{Environment.NewLine}" +
                    "Придумайте другое имя.", WindowType.VariableEditor);
                return;
            }

            var success = _variablesRepository.AddOrUpdate(Switcher.Variable,
                Switcher.NameIsChanged ? Switcher.OldName : null);
            if (!success)
            {
                await _dialogsHelper.ShowMessage(_variablesRepository.ErrorMessage, WindowType.VariableEditor);
            }
            else Close();
        }

        private async void DiscardChanges()
        {
            if (Switcher.HasChanges)
            {
                if (await _dialogsHelper.ShowRequest("Сохранить изменения?", WindowType.VariableEditor))
                {
                    SaveChanges();
                    Close();
                }
            }
            Close();
        }
    }
}