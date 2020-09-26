using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Messaging;
using MaterialDesignThemes.Wpf;
using TestApp.Models;
using TestApp.Repository;
using TestApp.ViewModels.Dialogs;
using TestApp.ViewModels.Interfaces;
using TestApp.ViewModels.Windows;
using TestApp.Views.Dialogs;
using TestApp.Views.Windows;
using Task = System.Threading.Tasks.Task;
using VariableEditor = TestApp.Views.Dialogs.VariableEditor;

namespace TestApp.ViewModels.Helpers
{
    public enum FileType
    {
        Any,
        Word,
        Xml  
    }

    public class DialogsManager
    {
        private readonly DataProvider _dataProvider;
        private MessageDialogViewModel _messageViewModel = new MessageDialogViewModel("");

        public SnackbarMessageQueue MessageQueue { get; set; } = new SnackbarMessageQueue(TimeSpan.FromSeconds(2));

        public DialogsManager(DataProvider dataProvider)
        {
            _dataProvider = dataProvider;
        }

        public void ShowNotification(string message)
        {
            MessageQueue.Enqueue(message, "OK", () => { });
        }

        /// <summary>
        /// Отображает окно запроса с кнопками Да / Нет
        /// </summary>
        public async Task<bool> ShowRequest(string text, WindowType hostIdentifier = WindowType.Root)
        {
            try
            {
                var viewModel = new MessageDialogViewModel(text, false);
                var view = new MessageDialog {ViewModel = viewModel};

                object ret = null;

                await DialogHost.Show(view, Common.GetEnumDescription(hostIdentifier), (o, args) =>
                {
                    ret = args.Parameter;
                });

                if (ret == null) return false;
                return (bool) ret;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        /// <summary>
        /// Выводит простое сообщение
        /// </summary>
        public async Task ShowMessage(string text, WindowType hostIdentifier = WindowType.Root)
        {
            try
            {
                var viewModel = new MessageDialogViewModel(text);
                var view = new MessageDialog { ViewModel = viewModel };

                object ret = null;

                //var r = ViewModelLocator.Current.MessageDialog;

                await DialogHost.Show(view, Common.GetEnumDescription(hostIdentifier), (o, args) =>
                {
                    ret = args.Parameter;
                });

            }
            catch (Exception e)
            {
            }
        }

        /// <summary>
        /// Открывает Редактор переменных
        /// </summary>
        public async Task OpenVariableEditor(
            VariablesRepository repository,
            IVariableWrapper variable = null,
            WindowType hostIdentifier = WindowType.Root)
        {
            var viewModel = new VariableEditorViewModel(_dataProvider, variable);
            var view = new VariableEditor {ViewModel = viewModel};

            await DialogHost.Show(view, Common.GetEnumDescription(hostIdentifier));
        }

        /// <summary>
        /// Открывает Редактор действий для редактирования действия
        /// </summary>
        public async Task<ObjectAction> OpenActionEditor(
            ObjectAction selectedAction,
            IEnumerable<ObjectAction> actions,
            ObjectsRepository objectsRepository, VariablesRepository variablesRepository,
            WindowType hostIdentifier = WindowType.Root)
        {
            var viewModel = new ActionEditorViewModel(selectedAction.Clone(), actions, _dataProvider);
            var view = new ActionEditor {ViewModel = viewModel};

            await DialogHost.Show(view, Common.GetEnumDescription(hostIdentifier));

            return viewModel.IsSaved ? viewModel.Action : null;
        }

        /// <summary>
        /// Открывает Редактор действий для создания нового действия
        /// </summary>
        public async Task<ObjectAction> OpenActionEditor(
            IVariableWrapper variable,
            IEnumerable<ObjectAction> actions,
            ObjectsRepository objectsRepository,
            VariablesRepository variablesRepository,
            string rootPath,
            WindowType hostIdentifier = WindowType.Root)
        {
            var viewModel = new ActionEditorViewModel(variable, actions, _dataProvider);
            var view = new ActionEditor {ViewModel = viewModel};

            await DialogHost.Show(view, Common.GetEnumDescription(hostIdentifier));

            return viewModel.IsSaved ? viewModel.Action : null;
        }



        public async Task ShowMenu(WindowType hostIdentifier = WindowType.Root)
        {
            var viewModel = new TreeMenuViewModel(_dataProvider);
            var view = new TreeMenu(viewModel);

            await DialogHost.Show(view, Common.GetEnumDescription(hostIdentifier));
        }

        public async Task<string> GetValue(string text, WindowType hostIdentifier = WindowType.Root,
            string value = null)
        {
            var viewModel = new InputDialogViewModel(text);
            if (value != null) viewModel.Value = value;

            var view = new InputDialog {ViewModel = viewModel};

            object ret = null;
            await DialogHost.Show(view, Common.GetEnumDescription(hostIdentifier), (o, args) =>
            {
                ret = args.Parameter;
            });

            return ret == null ? null : viewModel.Value;
        }



        public async Task ShowProgress(string text, WindowType hostIdentifier = WindowType.Root)
        {
            var viewModel = new ProgressDialogViewModel(text);
            var view = new ProgressDialog {ViewModel = viewModel};

            await DialogHost.Show(view, Common.GetEnumDescription(hostIdentifier));
        }





        public void ShowProgressAndContinue(string text, WindowType hostIdentifier = WindowType.Root)
        {
            var viewModel = new ProgressDialogViewModel(text);
            var view = new ProgressDialog {ViewModel = viewModel};
            DialogHost.Show(view, Common.GetEnumDescription(hostIdentifier));

            //var cts = new CancellationTokenSource();

            //await Task.Factory.StartNew(async () =>
            //{
            //    await DialogHost.Show(view, Common.GetEnumDescription(hostIdentifier));
            //}, cts.Token);

            //cts.CancelAfter(50000);
        }

        public void ChangeProgressText(string text)
        {
            Messenger.Default.Send(new NotificationMessage<string>(text, Messages.DoChangeProgressText));
        }


        public string OpenFileDialog(string fileName = null, FileType fileType = FileType.Any)
        {
            string filters;
            switch (fileType)
            {
                case FileType.Word:
                    filters = "Документы Microsoft Word|*.doc;*.docx";
                    break;
                case FileType.Xml:
                    filters = $"Файлы проекта|*.{Constants.ExtName}";
                    break;
                default:
                    filters = "Все файлы| *.*";
                    break;
            }

            var dialog = new Ookii.Dialogs.Wpf.VistaOpenFileDialog
            {
                CheckFileExists = true,
                ShowReadOnly = true,
                RestoreDirectory = true,
                DereferenceLinks = true,
                Filter = filters
            };

            if (string.IsNullOrEmpty(fileName))
            {
                dialog.InitialDirectory = Path.GetDirectoryName(fileName);
                dialog.FileName = Path.GetFileName(fileName);
            }
            else
            {
                // todo set default initial path
            }

            return dialog.ShowDialog().GetValueOrDefault() ? dialog.FileName : null;
        }

        public string OpenSaveDialog(FileType fileType = FileType.Xml, string initialPath = null)
        {
            string filters;
            string ext;
            switch (fileType)
            {
                case FileType.Word:
                    ext = "doc";
                    filters = "Документы Microsoft Word|*.doc;*.docx";
                    break;
                case FileType.Xml:
                    ext = Constants.ExtName;
                    filters = $"Файлы проекта|*.{ext}";
                    break;
                default:
                    ext = string.Empty;
                    filters = "Все файлы| *.*";
                    break;
            }

            var dialog = new Ookii.Dialogs.Wpf.VistaSaveFileDialog
            {
                CheckFileExists = true,
                RestoreDirectory = true,
                DereferenceLinks = true,
                Filter = filters
            };
            if (!string.IsNullOrEmpty(initialPath)) dialog.InitialDirectory = initialPath;

            if (!dialog.ShowDialog().GetValueOrDefault()) return null;

            var fileName = dialog.FileName;
            var fileExt = Path.GetExtension(fileName);
            if (string.IsNullOrEmpty(fileExt)) fileName += $".{ext}";

            return fileName;
        }


        public string OpenFolderDialog(string rootPath)
        {
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            if (!string.IsNullOrEmpty(rootPath)) dialog.SelectedPath = rootPath;

            return dialog.ShowDialog().GetValueOrDefault() ? dialog.SelectedPath : null;
        }



        public void CloseProgress()
        {
            Messenger.Default.Send(new NotificationMessage(Messages.DoCloseProgress));
        }


        public async Task OpenConfiguration(
            DataProvider dataProvider, byte selectedTabIndex = 0,
            WindowType hostIdentifier = WindowType.Root)
        {
            try
            {
                var viewModel = new ConfigViewModel(dataProvider) {SelectedTabIndex = selectedTabIndex};
                var view = new ConfigDialog {ViewModel = viewModel};

                await DialogHost.Show(view, Common.GetEnumDescription(hostIdentifier));

            }
            catch (Exception e)
            {
            }
        }

    }
}