using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using TestApp.Models;
using TestApp.Repository;
using TestApp.ViewModels.Dialogs.Menu;
using TestApp.ViewModels.Helpers;

namespace TestApp.ViewModels.Dialogs
{
    public class TreeMenuViewModel : ViewModelBase
    {
        private readonly ProjectRepository _projectRepository;
        private readonly string _rootPath;
        private readonly MenuItemProvider _itemProvider = new MenuItemProvider();

        public RelayCommand<string> ClickCommand => new RelayCommand<string>(OpenProject);
        public RelayCommand<object> AddItemCommand => new RelayCommand<object>(AddItem);
        public RelayCommand<object> AddGroupCommand => new RelayCommand<object>(AddGroup);
        public RelayCommand<object> RemoveElementCommand => new RelayCommand<object>(RemoveElement);

        public DialogsManager DialogsHelper { get; set; }

        public bool RemoveBtnIsVisible
        {
            get { return _removeBtnIsVisible; }
            set { Set(ref _removeBtnIsVisible, value); }
        }
        private bool _removeBtnIsVisible;

        public object SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                Set(ref _selectedItem, value);
                RemoveBtnIsVisible = value != null;
            }
        }
        private object _selectedItem;

        public ObservableCollection<MenuItem> Items { get; set; } = new ObservableCollection<MenuItem>();

        public TreeMenuViewModel(DataProvider dataProvider)
        {
            _projectRepository = dataProvider.ProjectRepository;
            DialogsHelper = dataProvider.DialogsManager;
            _rootPath = dataProvider.CommonSettings.RootPath;
            FillItems();
        }

        private async void AddItem(object selected)
        {
            var name = await GetName("Введите имя проекта.");
            if (name == null) return;

            string fileName;
            var ext = Constants.ExtName;
            if (selected == null)
            {
                fileName = _rootPath + $"\\{name}.{ext}";
            }
            else if (selected is DirectoryItem)
            {
                fileName = ((DirectoryItem) selected).Path + $"\\{name}.{ext}";
            }
            else if (selected is FileItem)
            {
                var path = Path.GetDirectoryName(((FileItem) selected).Path);
                fileName = path + $"\\{name}.{ext}";
            }
            else return;

            if (File.Exists(fileName))
            {
                DialogsHelper.ShowNotification($"Проект с именем \"{name}\" в этой группе уже существует.");
                return;
            }

            Close();
            _projectRepository.NewProject(fileName);
        }

        private async void AddGroup(object selected)
        {
            var name = await GetName("Введите имя группы");
            if (name == null) return;

            if (selected == null)
            {
                // Add to empty root
                Directory.CreateDirectory(Path.Combine(_rootPath, name));
            }
            else if (selected is DirectoryItem)
            {
                var path = Path.Combine(((DirectoryItem)selected).Path, name);
                try
                {
                    Directory.CreateDirectory(path);
                }
                catch (Exception e)
                {
                    await DialogsHelper.ShowMessage($"Не удалось создать папку \"{path}\"{Environment.NewLine}" +
                                                    $"{e.Message}");
                    return;
                }
            }
            else if (selected is FileItem)
            {
                var selector = (FileItem) selected;
                var path = Path.GetDirectoryName(selector.Path);
                if (path == null) return;
                Directory.CreateDirectory(Path.Combine(path, name));
            }
            else return;

            FillItems();
        }

        private async void RemoveElement(object selected)
        {
            if (selected == null) return;
            if (selected is DirectoryItem)
            {
                var path = ((DirectoryItem) selected).Path;
                var groupName = Path.GetFileName(path);

                if (!await DialogsHelper.ShowRequest(
                    $"Удалить группу \"{groupName}\" и все связанные с ней файлы?",
                    WindowType.TreeMenu)) return;

                var itemsCount = _itemProvider.GetItems(path).Count(t => t is FileItem);
                if (itemsCount > 0)
                {
                    if (!await DialogsHelper.ShowRequest(
                        $"Группа \"{groupName}\" содержит проекты ({itemsCount}).{Environment.NewLine}" +
                        "Удалить их без возможности восстановления?", WindowType.TreeMenu)) return;
                }

                if (!RemoveGroup(path))
                {
                    await DialogsHelper.ShowMessage("Ошибка удаления группы", WindowType.TreeMenu);
                    return;
                }
                FillItems();
            }
            else if (selected is FileItem)
            {
                var fileName = ((FileItem) selected).Path;
                var name = Path.GetFileNameWithoutExtension(fileName);
                if (!await DialogsHelper.ShowRequest(
                    $"Удалить проект \"{name}\" без возможности восстановления?", WindowType.TreeMenu)) return;

                if (!RemoveFile(fileName))
                {
                    await DialogsHelper.ShowMessage("Ошибка удаления файла проекта", WindowType.TreeMenu);
                    return;
                }
                FillItems();
            }
        }

        private void OpenProject(string fileName)
        {
            Close();
            _projectRepository.OpenProject(fileName);
        }

        private void FillItems()
        {
            var items = _itemProvider.GetItems(_rootPath);

            Items.Clear();
            items.ForEach(t => Items.Add(t));
        }

        private static void Close()
        {
            Messenger.Default.Send(new NotificationMessage(Messages.DoCloseTreeMenu));
        }

        private async Task<string> GetName(string text)
        {
            return await DialogsHelper.GetValue(text, WindowType.TreeMenu);
        }

        private static bool RemoveGroup(string path)
        {
            try
            {
                Directory.Delete(path, true);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        private static bool RemoveFile(string fileName)
        {
            try
            {
                File.Delete(fileName);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}