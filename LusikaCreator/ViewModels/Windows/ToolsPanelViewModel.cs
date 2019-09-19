using System.Collections.Generic;
using System.Windows.Media;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using TestApp.Models;
using TestApp.Models.Config;
using TestApp.Models.Enums;
using TestApp.Repository;
using TestApp.ViewModels.Interfaces;

namespace TestApp.ViewModels.Windows
{
    public class ToolsPanelViewModel : ViewModelBase
    {
        private readonly DataProvider _dataProvider;
        private readonly CommonSettings _commonSettings;
        private readonly ObjectsRepository _objectsRepository;

        public RelayCommand AddLabelCommand => new RelayCommand(AddLabel);
        public RelayCommand AddTextBoxCommand => new RelayCommand(AddTextBox);
        public RelayCommand AddCheckBoxCommand => new RelayCommand(AddCheckBox);
        public RelayCommand AddComboBoxCommand => new RelayCommand(AddComboBox);
        public RelayCommand AddRadioButtonCommand => new RelayCommand(AddRadioButton);
        public RelayCommand ChangeModeCommand => new RelayCommand(ChangeMode);
        public RelayCommand AddDatePickerCommand => new RelayCommand(AddDatePicker);
        public RelayCommand AddTimePickerCommand => new RelayCommand(AddTimePicker);

        public WindowSettings Settings { get; set; }

        public string DebugBtnKind
        {
            get { return _debugBtnKind; }
            set { Set(ref _debugBtnKind, value); }
        }
        private string _debugBtnKind;

        public Brush DebugBtnColor
        {
            get { return _debugBtnColor; }
            set { Set(ref _debugBtnColor, value); }
        }
        private Brush _debugBtnColor;

        public bool IsDebugMode
        {
            get { return _isDebugMode; }
            set
            {
                Set(ref _isDebugMode, value);
                if (value)
                {
                    DebugBtnKind = "Stop";
                    DebugBtnColor = Brushes.Red;
                }
                else
                {
                    DebugBtnKind = "Play";
                    DebugBtnColor = Brushes.Green;
                }
            }
        }
        private bool _isDebugMode;

        public FontSettings FontSettings
        {
            get { return _fontSettings; }
            set
            {
                Set(ref _fontSettings, value);
                if (value == null)
                {
                    FontSettingsIsNotEmpty = false;
                    IsBoldBtnEnabled = false;
                    IsItalicBtnEnabled = false;
                    IsUnderlinedBtnEnabled = false;
                    SelectedFontSize = 12;
                    return;
                }

                FontSettingsIsNotEmpty = true;
                IsBoldBtnEnabled = FontSettings.IsBold;
                IsItalicBtnEnabled = FontSettings.IsItalic;
                IsUnderlinedBtnEnabled = FontSettings.IsUnderlined;
                SelectedFontSize = FontSettings.Size;
            }
        }
        private FontSettings _fontSettings;

        public bool FontSettingsIsNotEmpty
        {
            get { return _fontSettingsIsNotEmpty; }
            set { Set(ref _fontSettingsIsNotEmpty, value); }
        }
        private bool _fontSettingsIsNotEmpty;

        public bool IsBoldBtnEnabled
        {
            get { return _isBoldBtnEnabled; }
            set
            {
                Set(ref _isBoldBtnEnabled, value);
                if (FontSettings == null) return;

                FontSettings.IsBold = value;
                if (_selected != null) _selected.Properties.FontSettings.IsBold = value;
            }
        }
        private bool _isBoldBtnEnabled;

        public bool IsItalicBtnEnabled
        {
            get { return _isItalicBtnEnabled; }
            set
            {
                Set(ref _isItalicBtnEnabled, value);
                if (FontSettings == null) return;

                FontSettings.IsItalic = value;
                if (_selected != null) _selected.Properties.FontSettings.IsItalic = value;
            }
        }
        private bool _isItalicBtnEnabled;

        public bool IsUnderlinedBtnEnabled
        {
            get { return _isUnderlinedBtnEnabled; }
            set
            {
                Set(ref _isUnderlinedBtnEnabled, value);
                if (FontSettings == null) return;

                FontSettings.IsUnderlined = value;
                if (_selected != null) _selected.Properties.FontSettings.IsUnderlined = value;
            }
        }
        private bool _isUnderlinedBtnEnabled;

        public List<byte> FontSizes { get; set; }

        public byte SelectedFontSize
        {
            get { return _selectedFontSize; }
            set
            {
                Set(ref _selectedFontSize, value);
                if (FontSettings == null) return;

                FontSettings.Size = value;
                if (_selected != null) _selected.Properties.FontSettings.Size = value;
            }
        }
        private byte _selectedFontSize;


        public ToolsPanelViewModel(DataProvider dataProvider)
        {
            _dataProvider = dataProvider;
            Settings = dataProvider.CommonSettings.ToolsPanelSettings;
            _objectsRepository = dataProvider.ObjectsRepository;
            _objectsRepository.SelectionChanged += OnObjectSelected;
            _commonSettings = dataProvider.CommonSettings;
            _commonSettings.AppModeChanged += delegate { IsDebugMode = _commonSettings.AppMode == AppMode.Debug; };
            IsDebugMode = false;
            FontSettings = new FontSettings();

            FontSizes = new List<byte>();
            for (byte i = 8; i < 26; i++) FontSizes.Add(i);
            SelectedFontSize = 12;
        }

        private void OnObjectSelected(IObjectViewModel viewModel)
        {
            if (viewModel?.Properties?.FontSettings == null)
            {
                var defaultSettings = new FontSettings();
                defaultSettings.SetDefault();

                IsBoldBtnEnabled = defaultSettings.IsBold;
                IsItalicBtnEnabled = defaultSettings.IsItalic;
                IsUnderlinedBtnEnabled = defaultSettings.IsUnderlined;
                SelectedFontSize = defaultSettings.Size;
                return;
            }
            _selected = viewModel;
            if (_selected?.Properties?.FontSettings == null) return;

            IsBoldBtnEnabled = _selected.Properties.FontSettings.IsBold;
            IsItalicBtnEnabled = _selected.Properties.FontSettings.IsItalic;
            IsUnderlinedBtnEnabled = _selected.Properties.FontSettings.IsUnderlined;
            SelectedFontSize = _selected.Properties.FontSettings.Size;
        }

        private IObjectViewModel _selected;

        private void AddLabel()
        {
            _dataProvider.ObjectsRepository.Add(ObjectType.Label);
            //Messenger.Default.Send(new NotificationMessage(Messages.AddLabel));
        }

        private void AddTextBox()
        {
            _dataProvider.ObjectsRepository.Add(ObjectType.TextBox);
        }

        private void AddCheckBox()
        {
            _dataProvider.ObjectsRepository.Add(ObjectType.CheckBox);
        }

        private  void AddComboBox()
        {
            _dataProvider.ObjectsRepository.Add(ObjectType.ComboBox);
        }

        private void AddRadioButton()
        {
            _dataProvider.ObjectsRepository.Add(ObjectType.RadioButton);
        }

        private void AddDatePicker()
        {
            _dataProvider.ObjectsRepository.Add(ObjectType.DatePicker);
        }

        private void AddTimePicker()
        {
            _dataProvider.ObjectsRepository.Add(ObjectType.TimePicker);
        }

        private void ChangeMode()
        {
            IsDebugMode = !IsDebugMode;

            _commonSettings.AppMode = IsDebugMode ? AppMode.Debug : AppMode.Editor;
            _objectsRepository.SwitchAppMode();
            _objectsRepository.Select(null);
        }
    }
}