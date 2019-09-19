using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using TestApp.Models;

namespace TestApp.ViewModels.Windows
{
    public class ScriptsViewerViewModel : ViewModelBase
    {
        private readonly DataProvider _dataProvider;

        public ObservableCollection<UserScript> Scripts { get; set; }

        public RelayCommand AddScriptCommand => new RelayCommand(AddScript);

        public RelayCommand EditScriptCommand => new RelayCommand(EditScript);

        public RelayCommand RemoveScriptCommand => new RelayCommand(RemoveScript);


        public string Title
        {
            get { return _title; }
            set { Set(ref _title, value); }
        }
        private string _title;

        public UserScript Selected
        {
            get { return _selected; }
            set { Set(ref _selected, value); }
        }
        private UserScript _selected;


        public ScriptsViewerViewModel(DataProvider dataProvider, UserScript script = null)
        {
            Title = script == null ? "Создание скрипта" : "Редактирование скрипта";

            _dataProvider = dataProvider;
            Scripts = dataProvider.ScriptsRepository.Scripts;
        }

        private void AddScript()
        {
        }

        private async void EditScript()
        {
            await _dataProvider.DialogsManager.OpenScriptEditor(_dataProvider.ScriptsRepository, Selected);
        }

        private void RemoveScript()
        {
        }
    }
}