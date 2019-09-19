using System.Windows;
using System.Windows.Media;
using GalaSoft.MvvmLight;

namespace TestApp.Models.Config
{   
    public class FontSettings : ViewModelBase
    {
        private readonly SolidColorBrush _defaultColor = Brushes.Navy;

        public byte Size
        {
            get { return _size; }
            set { Set(ref _size, value); }
        }
        private byte _size;

        public bool IsBold
        {
            get { return _isBold; }
            set
            {
                Set(ref _isBold, value);
                FontWeight = value ? FontWeights.Bold : FontWeights.Regular;
            }
        }
        private bool _isBold;

        public bool IsItalic
        {
            get { return _isItalic; }
            set
            {
                Set(ref _isItalic, value);
                FontStyle = value ? FontStyles.Italic : FontStyles.Normal;
            }
        }
        private bool _isItalic;

        public bool IsUnderlined
        {
            get { return _isUnderlined; }
            set
            {
                Set(ref _isUnderlined, value);
                TextDecoration = value ? TextDecorations.Underline : null;
            }
        }
        private bool _isUnderlined;

        public string HexColor
        {
            get { return _hexColor; }
            set
            {
                _hexColor = value;
                if (Color != null) return;

                try
                {
                    Color = (SolidColorBrush) new BrushConverter().ConvertFrom(value);
                }
                catch
                {
                    Color = _defaultColor;
                }
            }
        }
        private string _hexColor;

        public SolidColorBrush Color
        {
            get { return _color; }
            set
            {
                Set(ref _color, value);
                HexColor = value.Color.ToString();
            }
        }
        private SolidColorBrush _color;

        public FontWeight FontWeight
        {
            get { return _fontWeight; }
            set { Set(ref _fontWeight, value); }
        }
        private FontWeight _fontWeight;

        public FontStyle FontStyle
        {
            get { return _fontStyle; }
            set { Set(ref _fontStyle, value); }
        }
        private FontStyle _fontStyle;

        public TextDecorationCollection TextDecoration
        {
            get { return _textDecoration; }
            set { Set(ref _textDecoration, value); }
        }
        private TextDecorationCollection _textDecoration;

        public FontSettings()
        {
            SetDefault();
        }

        public void SetDefault()
        {
            Size = 12;
            IsBold = false;
            IsItalic = false;
            IsUnderlined = false;
            Color = _defaultColor;
        }

        public void Update(FontSettings fontSettings)
        {
            Size = fontSettings.Size;
            IsBold = fontSettings.IsBold;
            IsItalic = fontSettings.IsItalic;
            IsUnderlined = fontSettings.IsUnderlined;
            Color = fontSettings.Color;
        }

        public FontSettings Clone()
        {
            return new FontSettings
            {
                Size = Size,
                IsBold = IsBold,
                IsItalic = IsItalic,
                IsUnderlined = IsUnderlined,
                Color = Color
            };
        }

        public StoredFontSettings ToStoredObject()
        {
            return new StoredFontSettings
            {
                Size = Size,
                IsBold = IsBold,
                IsItalic = IsItalic,
                IsUnderlined = IsUnderlined,
                HexColor = HexColor
            };
        }
    }
}