using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using TestApp.Models;
using TestApp.ViewModels.Helpers;

namespace TestApp.ViewModels.Dialogs
{
    public class ConfigViewModel : ViewModelBase
    {
        private readonly DataProvider _dataProvider;

        public RelayCommand SelectWorkingPathCommand => new RelayCommand(SelectWorkingPath);
        public RelayCommand SelectTemplateCommand => new RelayCommand(SelectTemplate);
        public RelayCommand SelectOutPathCommand => new RelayCommand(SelectOutPath);
        public RelayCommand SaveCommand => new RelayCommand(SaveChanges);
        public RelayCommand CloseCommand => new RelayCommand(CloseDialog);

        public bool HasChanges
        {
            get { return _hasChanges; }
            set { Set(ref _hasChanges, value); }
        }
        private bool _hasChanges;

        public byte SelectedTabIndex
        {
            get { return _selectedTabIndex; }
            set { Set(ref _selectedTabIndex, value); }
        }
        private byte _selectedTabIndex;

        public string Title
        {
            get { return _title; }
            set
            {
                if (_title == value) return;

                Set(ref _title, value);
                if (!HasChanges) HasChanges = true;
            }
        }
        private string _title;

        public string WorkingPath
        {
            get { return _workingPath; }
            set
            {
                if (_workingPath == value) return;

                Set(ref _workingPath, value);
                HasChanges = true;
            }
        }
        private string _workingPath;

        public string TemplateFileName
        {
            get { return _templateFileName; }
            set
            {
                if (_templateFileName == value) return;
                Set(ref _templateFileName, value);
                if (!HasChanges) HasChanges = true;
            }
        }
        private string _templateFileName;

        public string OutPath
        {
            get { return _outPath; }
            set
            {
                if (_outPath == value) return;

                Set(ref _outPath, value);
                if (!HasChanges) HasChanges = true;
            }
        }
        private string _outPath;

        public string ResultFileName
        {
            get { return _resultFileName; }
            set
            {
                if (_resultFileName == value) return;

                Set(ref _resultFileName, value);
                if (!HasChanges) HasChanges = true;
            }
        }
        private string _resultFileName;

        public bool ExecuteFile
        {
            get { return _executeFile; }
            set
            {
                if (_executeFile == value) return;
                Set(ref _executeFile, value);
                if (value) OpenFolder = false;
                if (!HasChanges) HasChanges = true;
            }
        }
        private bool _executeFile;

        public bool OpenFolder
        {
            get { return _openFolder; }
            set
            {
                if (_openFolder == value) return;
                Set(ref _openFolder, value);
                if (value) ExecuteFile = false;
                if (!HasChanges) HasChanges = true;
            }
        }
        private bool _openFolder;

        public bool IsOverwrite
        {
            get { return _isOverwrite; }
            set
            {
                if (_isOverwrite == value) return;
                Set(ref _isOverwrite, value);
                if (!HasChanges) HasChanges = true;
            }
        }
        private bool _isOverwrite;


        private async void CloseDialog()
        {
            if (!HasChanges)
            {
                Close();
                return;
            }

            if (await _dataProvider.DialogsManager.ShowRequest("Сохранить изменения?", WindowType.ConfigDialog))
            {
                if (Save()) Close();
                else
                {
                    await _dataProvider.DialogsManager.ShowMessage("Ошибка сохранения общих настроек: " +
                                                                  $"{_dataProvider.ErrorMessage}",
                        WindowType.ConfigDialog);
                    return;
                }
            }
            Close();
        }

        private bool Save()
        {
            _dataProvider.CommonSettings.RootPath = WorkingPath;

            var projectSettings = _dataProvider.ProjectRepository.ProjectSettings;
            projectSettings.Title = Title;
            projectSettings.TemplateFileName = TemplateFileName;
            projectSettings.OutPath = OutPath;
            projectSettings.SavedFileName = ResultFileName;
            projectSettings.ShowTargetFile = ExecuteFile;
            projectSettings.OpenTargetFolder = OpenFolder;
            projectSettings.IsOverwrite = IsOverwrite;

            if (!_dataProvider.SaveCommonSettings())
            {
                HasChanges = false;
                return false;
                // todo message
            }

            return true;
        }

        private async void SaveChanges()
        {
            if (!Save())
            {
                await _dataProvider.DialogsManager.ShowMessage("Ошибка сохранения общих настроек: " +
                                                $"{_dataProvider.ErrorMessage}", WindowType.ConfigDialog);
                return;
            }
            Close();
        }

        private void Close()
        {
            Messenger.Default.Send(new NotificationMessage(Messages.DoCloseConfigEditor));
        }

        private void SelectWorkingPath()
        {
            var selectedPath = _dataProvider.DialogsManager.OpenFolderDialog(WorkingPath);
            if (string.IsNullOrEmpty(selectedPath)) return;

            WorkingPath = selectedPath;
        }

        private void SelectTemplate()
        {
            var selectedFileName = _dataProvider.DialogsManager.OpenFileDialog(TemplateFileName, FileType.Word);

            if (string.IsNullOrEmpty(selectedFileName)) return;
            TemplateFileName = selectedFileName;
        }

        private void SelectOutPath()
        {
            var selectedPath = _dataProvider.DialogsManager.OpenFolderDialog(OutPath);
            if (string.IsNullOrEmpty(selectedPath)) return;

            OutPath = selectedPath;
        }


        public ConfigViewModel(DataProvider dataProvider)
        {
            _dataProvider = dataProvider;
            WorkingPath = _dataProvider.CommonSettings.RootPath;

            var projectSettings = _dataProvider.ProjectRepository.ProjectSettings;
            Title = projectSettings.Title;
            TemplateFileName = projectSettings.TemplateFileName;
            OutPath = projectSettings.OutPath;
            ResultFileName = projectSettings.SavedFileName;
            ExecuteFile = projectSettings.ShowTargetFile;
            OpenFolder = projectSettings.OpenTargetFolder;
            IsOverwrite = projectSettings.IsOverwrite;

            HasChanges = false;
        }

    }
}