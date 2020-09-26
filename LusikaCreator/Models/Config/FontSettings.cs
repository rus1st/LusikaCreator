using System.Windows;
using System.Windows.Media;
using GalaSoft.MvvmLight;

namespace TestApp.Models.Config
{   
    public class FontSettings : ViewModelBase
    {
        public byte Size
        {
            get { return _size; }
            set { Set(ref _size, value); }
        }
        private byte _size;

        public bool Bold
        {
            get { return _bold; }
            set
            {
                Set(ref _bold, value);
                FontWeight = value ? FontWeights.Bold : FontWeights.Regular;
            }
        }
        private bool _bold;

        public bool Italic
        {
            get { return _italic; }
            set
            {
                Set(ref _italic, value);
                FontStyle = value ? FontStyles.Italic : FontStyles.Normal;
            }
        }
        private bool _italic;

        public bool Underline
        {
            get { return _underline; }
            set
            {
                Set(ref _underline, value);
                TextDecoration = value ? TextDecorations.Underline : null;
            }
        }
        private bool _underline;

        public string HexColor
        {
            get { return _hexColor; }
            set
            {
                _hexColor = value;

                try
                {
                    Color = (SolidColorBrush) new BrushConverter().ConvertFrom(value);
                }
                catch
                {
                    Color = Constants.DefaultColor;
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
                _hexColor = value.Color.ToString();
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

        public FontSettings(StoredFontSettings settings)
        {
            if (settings == null)
            {
                SetDefault();
                return;
            }
            Size = settings.Size;
            Bold = settings.Bold;
            Italic = settings.Italic;
            Underline = settings.Underline;
            HexColor = settings.HexColor;
        }

        public void SetDefault()
        {
            Size = 12;
            Bold = false;
            Italic = false;
            Underline = false;
            Color = Constants.DefaultColor;
        }

        public void Update(StoredFontSettings fontSettings)
        {
            if (fontSettings == null)
            {
                SetDefault();
                return;
            }

            Size = fontSettings.Size;
            Bold = fontSettings.Bold;
            Italic = fontSettings.Italic;
            Underline = fontSettings.Underline;
            HexColor = fontSettings.HexColor;
        }

        public void Update(FontSettings fontSettings)
        {
            Size = fontSettings.Size;
            Bold = fontSettings.Bold;
            Italic = fontSettings.Italic;
            Underline = fontSettings.Underline;
            Color = fontSettings.Color;
        }

        public FontSettings Clone()
        {
            return new FontSettings
            {
                Size = Size,
                Bold = Bold,
                Italic = Italic,
                Underline = Underline,
                Color = Color
            };
        }

        public StoredFontSettings ToStoredObject()
        {
            return new StoredFontSettings
            {
                Size = Size,
                Bold = Bold,
                Italic = Italic,
                Underline = Underline,
                HexColor = HexColor
            };
        }
    }
}