using System.Runtime.Serialization;
using TestApp.Models.Enums;

namespace TestApp.Models.Config
{
    [DataContract(Name = "commonSettings", Namespace = "")]
    public class CommonSettings
    {
        public event Handlers.EmptyHandler AppModeChanged;

        public AppMode AppMode
        {
            get { return _appMode; }
            set
            {
                _appMode = value;
                AppModeChanged?.Invoke();
            }
        }
        private AppMode _appMode;

        [DataMember(Name = "rootPath", Order = 0)]
        public string RootPath { get; set; }

        [DataMember(Name = "mainWindow", Order = 1)]
        public StoredWindowSettings StoredMainWindowSettings { get; set; }

        [DataMember(Name = "objectBrowser", Order = 2)]
        public StoredWindowSettings StoredObjectBrowserSettings { get; set; }

        [DataMember(Name = "toolsPanel", Order = 3)]
        public StoredWindowSettings StoredToolsPanelSettings { get; set; }

        [DataMember(Name = "variablesViewer", Order = 4)]
        public StoredWindowSettings StoredVariablesViewerSettings { get; set; }

        [DataMember(Name = "scriptsViewer", Order = 5)]
        public StoredWindowSettings StoredScriptsViewerSettings { get; set; }

        public bool IsSet { get; set; }

        public WindowSettings MainWindowSettings { get; set; } = new WindowSettings();
        public WindowSettings ObjectBrowserSettings { get; set; } = new WindowSettings();
        public WindowSettings ToolsPanelSettings { get; set; } = new WindowSettings();
        public WindowSettings VariablesViewerSettings { get; set; } = new WindowSettings();
        public WindowSettings ScriptsViewerSettings { get; set; } = new WindowSettings();

        public CommonSettings()
        {
            RootPath = System.AppDomain.CurrentDomain.BaseDirectory;
            AppMode = AppMode.Viewer;
        }

        public void ToStoredSettings()
        {
            StoredMainWindowSettings = new StoredWindowSettings(MainWindowSettings);
            StoredObjectBrowserSettings = new StoredWindowSettings(ObjectBrowserSettings);
            StoredToolsPanelSettings = new StoredWindowSettings(ToolsPanelSettings);
            StoredVariablesViewerSettings = new StoredWindowSettings(VariablesViewerSettings);
            StoredScriptsViewerSettings = new StoredWindowSettings(ScriptsViewerSettings);
        }

        public void RestoreSettings()
        {
            MainWindowSettings = new WindowSettings(StoredMainWindowSettings);
            StoredMainWindowSettings = null;

            ObjectBrowserSettings = new WindowSettings(StoredObjectBrowserSettings);
            StoredObjectBrowserSettings = null;

            ToolsPanelSettings = new WindowSettings(StoredToolsPanelSettings);
            StoredToolsPanelSettings = null;

            VariablesViewerSettings = new WindowSettings(StoredVariablesViewerSettings);
            StoredVariablesViewerSettings = null;

            ScriptsViewerSettings = new WindowSettings(StoredScriptsViewerSettings);
            StoredScriptsViewerSettings = null;
        }
    }
}