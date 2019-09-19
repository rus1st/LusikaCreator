using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;
using TestApp.Models;

namespace TestApp.Repository
{
    public class ScriptsRepository : ViewModelBase
    {
        public ObservableCollection<UserScript> Scripts { get; set; } = new ObservableCollection<UserScript>();

        public string ErrorMessage { get; set; }

        public string FileName
        {
            get { return _fileName; }
            set { Set(ref _fileName, value); }
        }
        private string _fileName;


        public ScriptsRepository()
        {
        }

        public bool AddScript(UserScript script)
        {
            Scripts.Add(script);
            return false;
        }

        public bool EditScript(UserScript script)
        {
            return false;
        }

        public void RemoveScript(UserScript script)
        {

        }

        /// <summary>
        /// Возвращает имя метода из блока кода
        /// </summary>
        private string GetFuncName()
        {
            return string.Empty;
        }

        public bool TryExecute()
        {
            return false;
        }
    }
}