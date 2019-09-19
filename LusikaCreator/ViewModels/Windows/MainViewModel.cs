using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using TestApp.Models;
using TestApp.Models.Config;
using TestApp.Models.Enums;
using TestApp.Repository;
using TestApp.ViewModels.Helpers;

namespace TestApp.ViewModels.Windows
{
    public class MainViewModel : ViewModelBase
    {
        private readonly DataProvider _dataProvider;
        private readonly ProjectRepository _projectRepository;

        public RelayCommand ShowMenuCommand => new RelayCommand(ShowMenu);
        public RelayCommand NewCommand => new RelayCommand(New);
        public RelayCommand OpenCommand => new RelayCommand(Open);
        public RelayCommand SaveCommand => new RelayCommand(Save);
        public RelayCommand SaveAsCommand => new RelayCommand(SaveAs);
        public RelayCommand CloseCommand => new RelayCommand(ExitApp);
        public RelayCommand FillTemplateCommand => new RelayCommand(FillTemplate);
        public RelayCommand ResetTemplateCommand => new RelayCommand(ResetTemplate);
        public RelayCommand OpenConfigCommand => new RelayCommand(OpenConfig);
        public RelayCommand<Type> ChangeVisibilityCommand => new RelayCommand<Type>(ChangeVisibility);
        public RelayCommand SwitchToDebugModeCommand => new RelayCommand(SwitchToDebugMode);
        public RelayCommand SwitchToViewModeCommand => new RelayCommand(SwitchToViewMode);
        public RelayCommand AddTabCommand => new RelayCommand(AddTab);
        public RelayCommand HelpCommand => new RelayCommand(Help);
        public RelayCommand AboutCommand => new RelayCommand(About);

        public byte SelectedTabIndex
        {
            get { return _dataProvider.TabsRepository.SelectedTabIndex; }
            set { _dataProvider.TabsRepository.SelectedTabIndex = value; }
        }

        private void AddTab()
        {
            _dataProvider.TabsRepository.AddItem();
        }

        private void SwitchToDebugMode()
        {
            _dataProvider.CommonSettings.AppMode = AppMode.Editor;
            _dataProvider.WindowsManager.ToDebugMode();
        }

        private void SwitchToViewMode()
        {
            _dataProvider.CommonSettings.AppMode = AppMode.Viewer;
            _dataProvider.WindowsManager.ToViewMode();
        }


        private void ChangeVisibility(Type type)
        {
            var helper = _dataProvider.WindowsManager;
            if (type == typeof (ObjectBrowserViewModel))
            {
                var isVisible = _dataProvider.CommonSettings.ObjectBrowserSettings.IsVisible;
                if (isVisible) helper.ShowObjectBrowser();
                else helper.HideObjectBrowser();
            }
            else if (type == typeof (ToolsPanelViewModel))
            {
                var isVisible = _dataProvider.CommonSettings.ToolsPanelSettings.IsVisible;
                if (isVisible) helper.ShowToolsPanel();
                else helper.HideToolsPanel();
            }
            else if (type == typeof (VariablesViewerViewModel))
            {
                var isVisible = _dataProvider.CommonSettings.VariablesViewerSettings.IsVisible;
                if (isVisible) helper.ShowVariablesViewer();
                else helper.HideVariablesViewer();
            }
            else if (type == typeof(ScriptsViewerViewModel))
            {
                DialogsHelper.ShowNotification("В разработке");
                return;
                var isVisible = _dataProvider.CommonSettings.ScriptsViewerSettings.IsVisible;
                if (isVisible) helper.ShowScriptsViewer();
                else helper.HideScriptsViewer();
            }
        }

        public DialogsManager DialogsHelper { get; set; }

        public CommonSettings CommonSettings
        {
            get { return _commonSettings; }
            set
            {
                _commonSettings = value;
                RaisePropertyChanged();
            }
        }
        private CommonSettings _commonSettings;

        public ProjectSettings Settings
        {
            get { return _projectRepository.ProjectSettings; }
            set
            {
                _projectRepository.ProjectSettings = value;
                RaisePropertyChanged();
            }
        }

        public WindowSettings Position { get; set; }

        public ObservableCollection<MyTabItem> TabItems { get; set; }

        //public byte SelectedTabIndex
        //{
        //    get { return _dataProvider.TabsRepository.SelectedTabIndex; }
        //    set
        //    {
        //        _dataProvider.TabsRepository.SelectedTabIndex = value;
        //        RaisePropertyChanged();
        //    }
        //}

        public bool GridIsVisible
        {
            get { return _gridIsVisible; }
            set { Set(ref _gridIsVisible, value); }
        }
        private bool _gridIsVisible;

        public bool IsInEditMode
        {
            get { return _isInEditMode; }
            set { Set(ref _isInEditMode, value); }
        }
        private bool _isInEditMode;


        public MainViewModel(DataProvider dataProvider)
        {
            _dataProvider = dataProvider;
            _projectRepository = dataProvider.ProjectRepository;
            DialogsHelper = dataProvider.DialogsManager;
            CommonSettings = dataProvider.CommonSettings;
            Position = dataProvider.CommonSettings.MainWindowSettings;

            GridIsVisible = CommonSettings.AppMode == AppMode.Editor;
            Settings = _projectRepository.ProjectSettings;
            CommonSettings = CommonSettings;
            CommonSettings.AppModeChanged += delegate { GridIsVisible = CommonSettings.AppMode == AppMode.Editor; };
            TabItems = _dataProvider.TabsRepository.TabItems;

            IsInEditMode = _dataProvider.CommonSettings.AppMode == AppMode.Editor;
            _dataProvider.CommonSettings.AppModeChanged += delegate
            {
                IsInEditMode = _dataProvider.CommonSettings.AppMode == AppMode.Editor;
            };
        }

        private async void ShowMenu()
        {
            await DialogsHelper.ShowMenu();
        }

        public void Unselect()
        {
            Messenger.Default.Send(new NotificationMessage(Messages.DoUnselectAll));
        }

        private void New()
        {
            _projectRepository.NewProject();
            RaisePropertyChanged("SelectedTabIndex");
        }

        private async void FillTemplate()
        {
            if (!_projectRepository.ProjectSettings.IsCompleted)
            {
                DialogsHelper.ShowNotification("Заполните все обязательные поля.");
                await DialogsHelper.OpenConfiguration(_dataProvider, 1);
                return;
            }

            //await DialogsManager.ShowProgress("Формирование файла...");
            if (!_dataProvider.ObjectsRepository.IsCompleted())
            {
                DialogsHelper.ShowNotification($"Заполните все обязательные поля ({_dataProvider.ObjectsRepository.ErrorMessage}).");
                return;
            }

            if (!_projectRepository.FillTemplate())
            {
                await DialogsHelper.ShowMessage(_projectRepository.ErrorMessage);
            }

            DialogsHelper.CloseProgress();
        }

        private async void ResetTemplate()
        {
            await _projectRepository.OpenProject(_projectRepository.SelectedFileName);
        }

        private async void Open()
        {
            var fileName = DialogsHelper.OpenFileDialog(fileType: FileType.Xml);
            if (string.IsNullOrEmpty(fileName)) return;

            await _projectRepository.OpenProject(fileName);
            RaisePropertyChanged("SelectedTabIndex");
        }

        private async void Save()
        {
            var success = await _projectRepository.SaveProject();
            if (!string.IsNullOrEmpty(_projectRepository.ErrorMessage))
            {
                await DialogsHelper.ShowMessage($"Ошибка сохранения проекта: {_projectRepository.ErrorMessage}.");
            }
            else DialogsHelper.ShowNotification(success ? "Проект сохранен." : "Сохранение отменено.");
        }

        private async void SaveAs()
        {
            var fileName = DialogsHelper.OpenSaveDialog();
            if (string.IsNullOrEmpty(fileName)) return;

            var success = await _projectRepository.SaveProject(fileName);
            if (!string.IsNullOrEmpty(_projectRepository.ErrorMessage))
            {
                await DialogsHelper.ShowMessage($"Ошибка сохранения проекта: {_projectRepository.ErrorMessage}.");
            }
            else DialogsHelper.ShowNotification(success ? "Проект сохранен." : "Сохранение отменено.");
        }

        private async void ExitApp()
        {
            if (await DialogsHelper.ShowRequest("Выйти из программы?")) Application.Current.Shutdown();
        }

        private async void OpenConfig()
        {
            await DialogsHelper.OpenConfiguration(_dataProvider);
        }

        private async void Help()
        {
            var appInfo = $"Версия {Constants.Version}.{Environment.NewLine}Обновлено {Constants.LastUpdated}.";
            try
            {
                await DialogsHelper.ShowMessage(appInfo);
            }
            catch (Exception e)
            {
                DialogsHelper.ShowNotification(appInfo);
            }
        }

        private void About()
        {
            var helpFile = Common.GetInstallPath + "help.pdf";
            if (!File.Exists(helpFile))
            {
                DialogsHelper.ShowNotification("Файл справки не найден.");
                return;
            }
            System.Diagnostics.Process.Start(helpFile);
        }
    }
}