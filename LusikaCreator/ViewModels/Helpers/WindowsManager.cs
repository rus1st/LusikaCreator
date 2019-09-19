using System;
using System.ComponentModel;
using System.Windows;
using TestApp.Models;
using TestApp.Models.Enums;
using TestApp.ViewModels.Windows;
using TestApp.Views.Windows;
using Application = System.Windows.Application;
using MainWindow = TestApp.Views.Windows.MainWindow;

namespace TestApp.ViewModels.Helpers
{
    public class WindowsManager
    {
        private readonly DataProvider _dataProvider;
        private MainWindow _mainWindow;
        private ToolsPanel _toolsPanel;
        private ObjectBrowser _objectBrowser;
        private VariablesViewer _variablesViewer;
        private ScriptsViewer _scriptsViewer;

        public WindowsManager(DataProvider dataProvider)
        {
            _dataProvider = dataProvider;
        }

        public async void CreateWindows()
        {
            const int margin = 5;
            const int leftOffset = 150;
            const int topOffset = 150;

            var isInEditMode = _dataProvider.CommonSettings.AppMode == AppMode.Editor;

            var mainViewModel = new MainViewModel(_dataProvider);
            _mainWindow = new MainWindow(mainViewModel);

            var objectBrowserViewModel = new ObjectBrowserViewModel(_dataProvider);
            _objectBrowser = new ObjectBrowser(objectBrowserViewModel) {Height = _mainWindow.Height};

            var toolsPanelViewModel = new ToolsPanelViewModel(_dataProvider);
            _toolsPanel = new ToolsPanel(toolsPanelViewModel);

            var variablesViewerViewModel = new VariablesViewerViewModel(_dataProvider);
            _variablesViewer = new VariablesViewer(variablesViewerViewModel);

            var scriptsViewerViewModel = new ScriptsViewerViewModel(_dataProvider);
            _scriptsViewer = new ScriptsViewer(scriptsViewerViewModel);


            if (_dataProvider.CommonSettings.ObjectBrowserSettings.IsSet)
            {
                _objectBrowser.Left = _dataProvider.CommonSettings.ObjectBrowserSettings.Left;
                _objectBrowser.Top = _dataProvider.CommonSettings.ObjectBrowserSettings.Top;
            }
            else
            {
                _objectBrowser.Left = leftOffset;
                _objectBrowser.Top = topOffset + _toolsPanel.Height + margin;
            }

            _toolsPanel.Width = _objectBrowser.Width + _mainWindow.Width + margin;
            if (_dataProvider.CommonSettings.ToolsPanelSettings.IsSet)
            {
                _toolsPanel.Left = _dataProvider.CommonSettings.ToolsPanelSettings.Left;
                _toolsPanel.Top = _dataProvider.CommonSettings.ToolsPanelSettings.Top;
            }
            else
            {
                _toolsPanel.Left = leftOffset;
                _toolsPanel.Top = topOffset;
            }

            _variablesViewer.Height = _toolsPanel.Height + _mainWindow.Height + margin;
            if (_dataProvider.CommonSettings.VariablesViewerSettings.IsSet)
            {
                _variablesViewer.Left = _dataProvider.CommonSettings.VariablesViewerSettings.Left;
                _variablesViewer.Top = _dataProvider.CommonSettings.VariablesViewerSettings.Top;
            }
            else
            {
                _variablesViewer.Left = _mainWindow.Left + _mainWindow.Width + margin;
                _variablesViewer.Top = _toolsPanel.Top;
            }

            if (_dataProvider.CommonSettings.MainWindowSettings.IsSet)
            {
                _mainWindow.Left = _dataProvider.CommonSettings.MainWindowSettings.Left;
                _mainWindow.Top = _dataProvider.CommonSettings.MainWindowSettings.Top;
            }
            else
            {
                _mainWindow.Left = leftOffset + _objectBrowser.Width + margin;
                _mainWindow.Top = topOffset + _toolsPanel.Height + margin;
            }

            Application.Current.MainWindow = _mainWindow;
            _mainWindow.Activated += GotFocus;
            _mainWindow.Closing += OnClose;
            _mainWindow.StateChanged += OnStateChanged;


            _toolsPanel.Closing += (sender, e) =>
            {
                e.Cancel = true;
                HideToolsPanel();
            };
            _objectBrowser.Closing += (sender, e) =>
            {
                e.Cancel = true;
                HideObjectBrowser();
            };
            _variablesViewer.Closing += (sender, e) =>
            {
                e.Cancel = true;
                HideVariablesViewer();
            };
            _scriptsViewer.Closing += (sender, e) =>
            {
                e.Cancel = true;
                HideScriptsViewer();
            };

            _toolsPanel.Hide();
            _variablesViewer.Hide();
            _scriptsViewer.Hide();
            _objectBrowser.Hide();
            CenterMainWindow();

            _mainWindow.Show();
            SetVisibility();

            if (!_dataProvider.CommonSettings.IsSet) await _dataProvider.DialogsManager.OpenConfiguration(_dataProvider);
            else if (!isInEditMode) await _dataProvider.DialogsManager.ShowMenu();
        }

        private void CenterMainWindow()
        {
            var screenWidth = SystemParameters.PrimaryScreenWidth;
            var screenHeight = SystemParameters.PrimaryScreenHeight;
            var windowWidth = _mainWindow.Width;
            var windowHeight = _mainWindow.Height;
            _mainWindow.Left = screenWidth/2 - windowWidth/2;
            _mainWindow.Top = screenHeight/2 - windowHeight/2;
        }

        private void OnStateChanged(object sender, EventArgs e)
        {
            var state = ((MainWindow) sender).WindowState;
            if (state == WindowState.Minimized)
            {
                MinimizeAll();
            }
            else if (state == WindowState.Normal)
            {
                RestoreAll();
            }
        }

        private void MinimizeAll()
        {
            _toolsPanel.WindowState = WindowState.Minimized;
            _objectBrowser.WindowState = WindowState.Minimized;
            _variablesViewer.WindowState = WindowState.Minimized;
        }

        private void RestoreAll()
        {
            _toolsPanel.WindowState = WindowState.Normal;
            _objectBrowser.WindowState = WindowState.Normal;
            _variablesViewer.WindowState = WindowState.Normal;
        }

        private async void OnClose(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            if (!await _dataProvider.DialogsManager.ShowRequest($"Выйти из программы?{Environment.NewLine}" +
                                                               "Все не сохраненные данные будут утеряны.")) return;

            _dataProvider.XmlRepository.SaveCommonSettings();
            Application.Current.Shutdown();
        }

        private void GotFocus(object sender, EventArgs e)
        {
            _mainWindow.Topmost = true;
            _toolsPanel.Topmost = true;
            _objectBrowser.Topmost = true;
            _variablesViewer.Topmost = true;
            _mainWindow.Topmost = false;
            _toolsPanel.Topmost = false;
            _objectBrowser.Topmost = false;
            _variablesViewer.Topmost = false;
        }

        public void SetVisibility()
        {
            var isInEditMode = _dataProvider.CommonSettings.AppMode == AppMode.Editor;

            if (!isInEditMode || !_dataProvider.CommonSettings.ToolsPanelSettings.IsVisible) HideToolsPanel();
            else ShowToolsPanel();

            if (!isInEditMode || !_dataProvider.CommonSettings.ObjectBrowserSettings.IsVisible) HideObjectBrowser();
            else ShowObjectBrowser();

            if (!isInEditMode || !_dataProvider.CommonSettings.VariablesViewerSettings.IsVisible) HideVariablesViewer();
            else ShowVariablesViewer();
        }

        public void ToDebugMode()
        {
            ShowToolsPanel();
            ShowVariablesViewer();
            ShowObjectBrowser();
        }

        public void ToViewMode()
        {
            HideToolsPanel();
            HideVariablesViewer();
            HideObjectBrowser();
            CenterMainWindow();
        }

        public void HideToolsPanel()
        {
            _toolsPanel.Hide();
            _dataProvider.CommonSettings.ToolsPanelSettings.IsVisible = false;
        }
        public void ShowToolsPanel()
        {
            _toolsPanel.Show();
            _dataProvider.CommonSettings.ToolsPanelSettings.IsVisible = true;
        }

        public void HideObjectBrowser()
        {
            _objectBrowser.Hide();
            _dataProvider.CommonSettings.ObjectBrowserSettings.IsVisible = false;
        }
        public void ShowObjectBrowser()
        {
            _objectBrowser.Show();
            _dataProvider.CommonSettings.ObjectBrowserSettings.IsVisible = true;
        }

        public void HideVariablesViewer()
        {
            _variablesViewer.Hide();
            _dataProvider.CommonSettings.VariablesViewerSettings.IsVisible = false;
        }
        public void ShowVariablesViewer()
        {
            _variablesViewer.Show();
            _dataProvider.CommonSettings.VariablesViewerSettings.IsVisible = true;
        }

        public void HideScriptsViewer()
        {
            _scriptsViewer.Hide();
            _dataProvider.CommonSettings.ScriptsViewerSettings.IsVisible = false;
        }
        public void ShowScriptsViewer()
        {
            _scriptsViewer.Show();
            _dataProvider.CommonSettings.ScriptsViewerSettings.IsVisible = true;
        }
    }
}