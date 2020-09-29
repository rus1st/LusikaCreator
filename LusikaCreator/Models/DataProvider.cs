using TestApp.Models.Enums;
using TestApp.Models.Config;
using TestApp.Repository;
using TestApp.ViewModels.Helpers;
using TestApp.Models.Helpers;

namespace TestApp.Models
{
    public class DataProvider
    {
        public string ErrorMessage { get; set; }

        public CommonSettings CommonSettings { get; set; }

        public VariablesRepository VariablesRepository { get; }

        public ObjectsRepository ObjectsRepository { get; }

        public ProjectRepository ProjectRepository { get; }

        public TabsRepository TabsRepository { get; }

        public XmlRepository XmlRepository { get; }

        public DialogsManager DialogsManager { get; }

        public WindowsManager WindowsManager { get; }

        public TimeNotifier TimeNotifier { get; }


        public DataProvider(AppMode appMode)
        {
            XmlRepository = new XmlRepository(this);
            CommonSettings = XmlRepository.LoadCommonSettings();
            if (CommonSettings == null) CommonSettings = new CommonSettings {IsSet = false};
            else CommonSettings.IsSet = true;
            VariablesRepository = new VariablesRepository(this);
            ObjectsRepository = new ObjectsRepository(this);

            CommonSettings.AppMode = appMode;
            CommonSettings.AppModeChanged += delegate { ObjectsRepository.SwitchAppMode(); };
            
            ProjectRepository = new ProjectRepository(this);
            DialogsManager = new DialogsManager(this);
            TabsRepository = new TabsRepository(this);
            WindowsManager = new WindowsManager(this);
            TimeNotifier = new TimeNotifier();
            TimeNotifier.StartWatching();
        }

        public bool SaveCommonSettings()
        {
            return XmlRepository.SaveCommonSettings();
        }
    }
}