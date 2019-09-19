using System;
using System.Collections.Generic;
using System.Linq;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using TestApp.Models;
using TestApp.Models.Enums;
using TestApp.Repository;
using TestApp.ViewModels.Helpers;

namespace TestApp.ViewModels.Dialogs
{
    public class ScriptEditorViewModel : ViewModelBase
    {
        private readonly UserScript _userScript;
        private readonly DialogsManager _dialogsHelper;

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

        public string Description
        {
            get { return _description; }
            set
            {
                Set(ref _description, value);
                if (_description != value && !HasChanges) HasChanges = true;
            }
        }
        private string _description;

        public string Code
        {
            get { return _code; }
            set
            {
                Set(ref _code, value);
                if (_code != value && !HasChanges) HasChanges = true;
            }
        }
        private string _code;

        public VariableType InputType
        {
            get { return _inputType; }
            set
            {
                Set(ref _inputType, value);
                if (_inputType != value && !HasChanges) HasChanges = true;
            }
        }
        private VariableType _inputType;

        public VariableType OutputType
        {
            get { return _outputType; }
            set
            {
                Set(ref _outputType, value);
                if (_outputType != value && !HasChanges) HasChanges = true;
            }
        }
        private VariableType _outputType;

        public RelayCommand SaveCommand => new RelayCommand(SaveChanges);
        public RelayCommand DiscardCommand => new RelayCommand(DiscardChanges);

        public List<VariableType> AvailableTypes => Enum.GetValues(typeof(VariableType))
            .Cast<VariableType>()
            .Where(t => t != VariableType.Any && t != VariableType.Unknown)
            .ToList();


        public ScriptEditorViewModel(UserScript script, DataProvider dataProvider)
        {
            Title = "Создание скрипта";
            _userScript = script;
            _dialogsHelper = dataProvider.DialogsManager;
            HasChanges = false;
            InputType = VariableType.String;
            OutputType = VariableType.String;
        }

        private void SaveChanges()
        {
            _userScript.Description = Description;
            _userScript.Code = Code;
            _userScript.InputType = InputType;
            _userScript.OutputType = OutputType;
        }

        private async void DiscardChanges()
        {
            if (HasChanges)
            {
                var needSave = await _dialogsHelper.ShowRequest("Сохранить изменения?");
                if (needSave) SaveChanges();
            }
            CloseDialog();
        }

        private static void CloseDialog()
        {
            Messenger.Default.Send(new NotificationMessage(Messages.DoCloseScriptEditor));
        }
    }
}