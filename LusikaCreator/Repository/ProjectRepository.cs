using System;
using System.IO;
using System.Threading.Tasks;
using TestApp.Models;
using TestApp.Models.Config;
using TestApp.Models.Enums;
using TestApp.ViewModels.Helpers;

namespace TestApp.Repository
{
    public class ProjectRepository
    {
        private readonly DataProvider _dataProvider;

        public string SelectedFileName { get; set; }

        public ProjectSettings ProjectSettings { get; set; }

        public bool HasChanges { get; set; }

        public string ErrorMessage { get; set; }

        public ProjectRepository(DataProvider dataProvider)
        {
            _dataProvider = dataProvider;
            ProjectSettings = new ProjectSettings(_dataProvider.VariablesRepository);
            SelectedFileName = null;
            HasChanges = false;
            _dataProvider.ObjectsRepository.ObjectChanged += delegate { HasChanges = true; };
            _dataProvider.VariablesRepository.VariableChanged += delegate { HasChanges = true; };
        }

        private async Task DoBeforeCloseProject()
        {
            if (HasChanges)
            {
                var doSave = await _dataProvider.DialogsManager
                    .ShowRequest($"Сохранить изменения в проекте \"{ProjectSettings.Title}\"?");
                if (doSave)
                {
                    _dataProvider.DialogsManager.ShowNotification(!await SaveProject()
                        ? "Ошибка сохранения проекта."
                        : "Проект сохранен.");
                }
            }
        }

        public async void NewProject(string fileName = null)
        {
            await DoBeforeCloseProject();

            _dataProvider.VariablesRepository.Clear();
            _dataProvider.ObjectsRepository.Clear();
            ProjectSettings.Update(new ProjectSettings(_dataProvider.VariablesRepository));
            if (!string.IsNullOrEmpty(fileName))
            {
                var name = Path.GetFileNameWithoutExtension(fileName);
                ProjectSettings.Title = name;
                SelectedFileName = fileName;
            }
            _dataProvider.TabsRepository.Reset();
            _dataProvider.CommonSettings.AppMode = AppMode.Editor;

            HasChanges = false;
        }

        public async Task<bool> OpenProject(string fileName)
        {
            await DoBeforeCloseProject();

            var xml = new XmlRepository(_dataProvider);
            var ret = xml.LoadProject(fileName);
            if (!ret)
            {
                ErrorMessage = "Ошибка загрузки проекта.";
                return false;
            }

            SelectedFileName = fileName;
            HasChanges = false;
            return true;
        }

        public async Task<bool> SaveProject(string fileName = null)
        {
            var targetFileName = fileName ?? SelectedFileName;
            if (string.IsNullOrEmpty(targetFileName))
            {
                var saveTo = _dataProvider.DialogsManager.OpenSaveDialog(FileType.Xml);
                if (saveTo == null)
                {
                    ErrorMessage = string.Empty;
                    return false;
                }
                if (File.Exists(saveTo))
                {
                    var isOverwrite = await _dataProvider.DialogsManager.ShowRequest("Перезаписать файл проекта?");
                    if (!isOverwrite) return false;
                }
                targetFileName = saveTo;
            }

            if (!IsCorrectFileName(Path.GetFileName(targetFileName)))
            {
                ErrorMessage = "Имя файла содержит недопустимые символы.";
                return false;
            }

            var xml = _dataProvider.XmlRepository;
            var ret = xml.SaveProject(targetFileName);
            if (!ret)
            {
                ErrorMessage = $"Ошибка импорта проекта \"{targetFileName}\"";
                if (!string.IsNullOrEmpty(xml.ErrorMessage)) ErrorMessage += $": {xml.ErrorMessage}";
                return false;
            }

            HasChanges = false;
            return true;
        }

        public bool FillTemplate()
        {
            var rep = new WordRepository(_dataProvider);

            if (!rep.FillTemplate(ProjectSettings.OutPath, ProjectSettings.SavedFileName))
            {
                ErrorMessage = $"Ошибка формирования файла{Environment.NewLine}{rep.ErrorMessage}";
                return false;
            }
            return true;
        }

        private static bool IsCorrectFileName(string fileName)
            => fileName.IndexOfAny(Path.GetInvalidFileNameChars()) < 0;
    }
}