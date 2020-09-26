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

        public WindowSettings WindowSettings { get; set; }

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

        public FontSettings FontSettings { get; set; }

        public bool FontIsSet
        {
            get { return _fontIsSet; }
            set { Set(ref _fontIsSet, value); }
        }
        private bool _fontIsSet;

        public List<byte> FontSizes { get; set; }

        public ToolsPanelViewModel(DataProvider dataProvider)
        {
            _dataProvider = dataProvider;
            WindowSettings = dataProvider.CommonSettings.ToolsPanelSettings;
            _objectsRepository = dataProvider.ObjectsRepository;
            _objectsRepository.SelectionChanged += OnObjectSelected;
            _objectsRepository.Unselected += OnUnselected;

            _commonSettings = dataProvider.CommonSettings;
            _commonSettings.AppModeChanged += delegate { IsDebugMode = _commonSettings.AppMode == AppMode.Debug; };
            IsDebugMode = false;

            FontSizes = new List<byte>();
            for (byte i = 8; i < 26; i++) FontSizes.Add(i);
            FontSettings = new FontSettings();
        }

        private void OnUnselected()
        {
            FontSettings = new FontSettings();
            FontIsSet = false;
            UpdateView();
        }

        public Color SelectedColor
        {
            get
            {
                if (FontSettings == null) return Constants.DefaultColor.Color;
                return FontSettings.Color.Color;
            }
            set
            {
                if (FontSettings == null) return;
                FontSettings.Color = new SolidColorBrush(value);
            }
        }

        private void OnObjectSelected(IObjectViewModel formObject)
        {
            if (formObject?.Properties == null)
            {
                OnUnselected();
                return;
            }

            if (formObject.Properties.FontSettings == null)
            {
                formObject.Properties.FontSettings = new FontSettings();
            }

            FontIsSet = true;
            FontSettings = formObject.Properties.FontSettings;
            UpdateView();
        }

        private void UpdateView()
        {
            RaisePropertyChanged(nameof(FontSettings));
            RaisePropertyChanged(nameof(SelectedColor));
        }

        private void AddLabel()
        {
            _dataProvider.ObjectsRepository.Add(ObjectType.Label);
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