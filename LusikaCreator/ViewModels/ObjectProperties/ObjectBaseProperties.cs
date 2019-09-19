using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using GalaSoft.MvvmLight;
using TestApp.Models.Config;
using TestApp.Models.Enums;
using TestApp.Repository;
using TestApp.ViewModels.Helpers;

namespace TestApp.ViewModels.ObjectProperties
{
    public class ObjectBaseProperties : ViewModelBase
    {
        private readonly ObjectsRepository _objectsRepository;

        protected AppMode AppMode
        {
            get { return _appMode; }
            set
            {
                if (_appMode == value) return;

                var isVisible = GetVisibility();
                Set(ref _appMode, value);

                IsInEditMode = value == AppMode.Editor;
                SetVisibility(isVisible);

                if (!IsInEditMode) IsSelected = false;
            }
        }
        private AppMode _appMode;

        public uint Id
        {
            get { return _id; }
            set
            {
                if (_id == value) return;
                Set(ref _id, value);
            }
        }
        private uint _id;

        public byte TabId
        {
            get { return _tabId; }
            set
            {
                if (_tabId == value) return;
                Set(ref _tabId, value);
            }
        }
        private byte _tabId;

        public string Name
        {
            get { return _name; }
            set
            {
                if (_name == value) return;
                Set(ref _name, value);
            }
        }
        private string _name;

        public int Left
        {
            get { return _left; }
            set
            {
                if (_left == value) return;
                Set(ref _left, value);
            }
        }
        private int _left;

        public int Top
        {
            get { return _top; }
            set
            {
                if (_top == value) return;
                Set(ref _top, value);
            }
        }
        private int _top;

        /// <summary>
        /// Определяет видимость объекта в режиме редактирования
        /// </summary>
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                if (_isEnabled == value) return;
                Set(ref _isEnabled, value);
            }
        }
        private bool _isEnabled;

        /// <summary>
        /// Определяет видимость объекта в режиме просмотра
        /// </summary>
        public bool IsVisible
        {
            get { return _isVisible; }
            set
            {
                if (_isVisible == value) return;
                Set(ref _isVisible, value);
            }
        }
        private bool _isVisible;

        /// <summary>
        /// Определяет, находится ли программа в режиме редактирования
        /// </summary>
        public bool IsInEditMode
        {
            get { return _isInEditMode; }
            set
            {
                if (_isInEditMode == value) return;
                Set(ref _isInEditMode, value);
            }
        }
        private bool _isInEditMode;

        public int ZIndex
        {
            get { return _zIndex; }
            set
            {
                if (_zIndex == value) return;
                Set(ref _zIndex, value);
            }
        }
        private int _zIndex;

        /// <summary>
        /// Видна ли рамка у объекта
        /// </summary>
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                Set(ref _isSelected, value);
                if (value)
                {
                    BorderDepth = 1;
                    BorderColor = Brushes.IndianRed;
                    ZIndex = 1;
                    IsHighlighted = true;
                }
                else
                {
                    BorderDepth = 0;
                    ZIndex = 0;
                    IsHighlighted = false;
                }
            }
        }
        private bool _isSelected;

        /// <summary>
        /// Подсвечен ли фон объекта
        /// </summary>
        public bool IsHighlighted
        {
            get { return _isHighlighted; }
            set
            {
                Set(ref _isHighlighted, value);
                BackgroundColor = value ? Brushes.OldLace : Brushes.White;
            }
        }
        private bool _isHighlighted;

        public int BorderDepth
        {
            get { return _borderDepth; }
            set
            {
                if (_borderDepth == value) return;
                Set(ref _borderDepth, value);
            }
        }
        private int _borderDepth;

        public Brush BorderColor
        {
            get { return _borderColor; }
            set
            {
                if (Equals(_borderColor, value)) return;
                Set(ref _borderColor, value);
            }
        }
        private Brush _borderColor;

        public Brush BackgroundColor
        {
            get { return _backgroundColor; }
            set
            {
                if (Equals(_backgroundColor, value)) return;
                Set(ref _backgroundColor, value);
            }
        }
        private Brush _backgroundColor;

        public FontSettings FontSettings
        {
            get { return _fontSettings; }
            set
            {
                if (_fontSettings == value) return;
                Set(ref _fontSettings, value);
            }
        }
        private FontSettings _fontSettings;

        public ObjectBaseProperties(uint id, string name, AppMode appMode,
            ObjectsRepository objectsRepository)
        {
            _objectsRepository = objectsRepository;
            AppMode = appMode;
            Id = id;
            Name = name;
            SetVisibility(true);
            RequestMove = new SimpleCommand<Point>(MoveTo);
        }

        /// <summary>
        /// Задает видимость объекта
        /// </summary>
        public void SetVisibility(bool isVisible)
        {
            if (IsInEditMode)
            {
                IsEnabled = isVisible;
                IsVisible = true;
            }
            else
            {
                IsEnabled = true;
                IsVisible = isVisible;
            }
        }

        public bool GetVisibility()
        {
            return IsInEditMode ? IsEnabled : IsVisible;
        }

        public void SwitchAppMode(AppMode appMode)
        {
            AppMode = appMode;
        }

        public void Select()
        {
            _objectsRepository.Select(Id);
        }

        public void Unselect()
        {
            if (IsSelected) IsSelected = false;
            if (IsHighlighted) IsHighlighted = false;
        }

        public void Highlight()
        {
            if (!IsHighlighted) IsHighlighted = true;
        }

        public ObjectBaseProperties Clone()
        {
            return new ObjectBaseProperties(Id, Name, AppMode, _objectsRepository)
            {
                Left = Left,
                Top = Top,
                IsSelected = IsSelected,
                IsHighlighted = IsHighlighted,
                IsEnabled = IsEnabled,
                IsVisible = IsVisible,
                //IsRequired = IsRequired,
                //BorderDepth = BorderDepth,
                //BorderColor = BorderColor,
                //BackgroundColor = BackgroundColor,
                FontSettings = FontSettings,
                TabId = TabId
            };
        }

        public void Update(ObjectBaseProperties buffer)
        {
            AppMode = buffer.AppMode;
            Id = buffer.Id;
            Name = buffer.Name;
            //Left = buffer.Left;
            //Top = buffer.Top;
            //ZIndex = buffer.ZIndex;
            IsSelected = buffer.IsSelected;
            IsHighlighted = buffer.IsHighlighted;
            IsEnabled = buffer.IsEnabled;
            IsVisible = buffer.IsVisible;
            //IsRequired = buffer.IsRequired;
            //BorderDepth = buffer.BorderDepth;
            //BorderColor = buffer.BorderColor;
            //BackgroundColor = buffer.BackgroundColor;
            FontSettings = buffer.FontSettings;
        }

        #region Draggable Options

        public ICommand RequestMove { get; }

        public Point Position
        {
            get { return _position; }
            set
            {
                Set(ref _position, value);
                Left = (int) Position.X;
                Top = (int) Position.Y;
            }
        }

        private Point _position;

        private void MoveTo(Point newPosition)
        {
            Left = (int) newPosition.X;
            Top = (int) newPosition.Y;
        }

        #endregion
    }
}