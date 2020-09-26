using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Data;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using TestApp.Models;
using TestApp.Models.Enums;
using TestApp.ViewModels.Interfaces;

namespace TestApp.Repository
{
    [DataContract(Name = "tab", Namespace = "")]
    public class StoredTabItem
    {
        [DataMember(Name = "id", Order = 0)]
        public byte Id { get; set; }

        [DataMember(Name = "text", Order = 1)]
        public string Text { get; set; }

        public StoredTabItem(byte id, string text)
        {
            Id = id;
            Text = text;
        }
    }

    public class MyTabItem : ViewModelBase
    {
        private readonly DataProvider _dataProvider;

        public RelayCommand RenameTabCommand => new RelayCommand(RenameTab);
        public RelayCommand RenameTabCompletedCommand => new RelayCommand(RenameTabCompleted);
        public RelayCommand<MyTabItem> CloseTabCommand => new RelayCommand<MyTabItem>(CloseTab);

        public byte Id { get; set; }

        public string Text
        {
            get { return _text; }
            set { Set(ref _text, value); }
        }
        private string _text;

        public bool IsRenaming
        {
            get { return _isRenaming; }
            set { Set(ref _isRenaming, value); }
        }
        private bool _isRenaming;

        public bool IsInEditMode
        {
            get { return _isInEditMode; }
            set { Set(ref _isInEditMode, value); }
        }
        private bool _isInEditMode;

        public CollectionViewSource FilteredObjects { get; set; } = new CollectionViewSource();

        public MyTabItem(byte id, string text, DataProvider dataProvider)
        {
            _dataProvider = dataProvider;
            Id = id;
            Text = text;
            FilteredObjects.Source = dataProvider.ObjectsRepository.ViewModels;
            FilteredObjects.Filter += Filter;
            IsRenaming = false;
            IsInEditMode = _dataProvider.CommonSettings.AppMode == AppMode.Editor;
            _dataProvider.CommonSettings.AppModeChanged += delegate
            {
                IsInEditMode = _dataProvider.CommonSettings.AppMode == AppMode.Editor;
                IsRenaming = false;
            };
        }

        private void Filter(object sender, FilterEventArgs e)
        {
            var vm = (IObjectViewModel) e.Item;
            var isAccepting = vm != null && vm.Properties.TabId == Id;

            e.Accepted = isAccepting;
        }

        private async void CloseTab(MyTabItem tabItem)
        {
            if (_dataProvider.TabsRepository.TabItems.Count == 1)
            {
                await _dataProvider.DialogsManager.ShowMessage("Нельзя удалить единственную вкладку.");
                return;
            }

            if (
                !await
                    _dataProvider.DialogsManager.ShowRequest("Удалить выбранную вкладку и все связанные с ней элементы?"))
                return;

            var assignedObjectsCount =
                _dataProvider.ObjectsRepository.ViewModels.Count(t => t.Properties.TabId == tabItem.Id);
            if (assignedObjectsCount != 0)
            {
                if (!await _dataProvider.DialogsManager.ShowRequest(
                    $"Выбранная вкладка содержит объекты ({assignedObjectsCount}).{Environment.NewLine}Продолжить?"))
                    return;
            }

            _dataProvider.TabsRepository.RemoveItem(tabItem);
            var removedObjects =
                _dataProvider.ObjectsRepository.ViewModels.Where(t => t.Properties.TabId == tabItem.Id).ToList();

            removedObjects.ForEach(t => _dataProvider.ObjectsRepository.Remove(t.Properties.Id));
        }

        private void RenameTab()
        {
            if (!IsInEditMode) return;
            IsRenaming = true;
        }

        private void RenameTabCompleted()
        {
            IsRenaming = false;
        }
    }

    public class TabsRepository : ViewModelBase
    {
        private readonly DataProvider _dataProvider;

        public ObservableCollection<MyTabItem> TabItems { get; set; } = new ObservableCollection<MyTabItem>();

        public byte SelectedTabIndex
        {
            get { return _selectedTabIndex; }
            set { Set(ref _selectedTabIndex, value); }
        }
        private byte _selectedTabIndex;

        public TabsRepository(DataProvider dataProvider)
        {
            _dataProvider = dataProvider;
            Reset();
        }

        public void Reset()
        {
            Clear();
            AddItem();
            SelectedTabIndex = 0;
        }

        public void Clear()
        {
            TabItems.Clear();
        }

        private byte GetId()
        {
            for (byte i = 0; i < byte.MaxValue; i++)
            {
                if (TabItems.FirstOrDefault(t => t.Id == i) != null) continue;
                return i;
            }
            return byte.MaxValue;
        }

        public void AddItem(StoredTabItem tab)
        {
            AddItem(tab.Id, tab.Text);
        }

        public void AddItem(byte id = byte.MaxValue, string text = null)
        {
            if (id == byte.MaxValue) id = GetId();

            if (string.IsNullOrEmpty(text)) text = GetName(id);
            if (TabItems.FirstOrDefault(t => t.Id == id) != null)
            {
                return;
            }

            TabItems.Add(new MyTabItem(id, text, _dataProvider));
        }

        private string GetName(byte id)
        {
            var name = $"Вкладка {++id}";
            if (TabItems.FirstOrDefault(t => Common.IsSameName(t.Text, name)) != null) name = "Новая вкладка";
            return name;
        }

        public void RemoveItem(MyTabItem tabItem)
        {
            var selector = TabItems.FirstOrDefault(t => t.Id == tabItem.Id);
            if (selector == null) return;

            TabItems.Remove(selector);
        }

        public List<StoredTabItem> ToStoredObject()
        {
            return TabItems.Select(tab => new StoredTabItem(tab.Id, tab.Text)).ToList();
        }
    }
}