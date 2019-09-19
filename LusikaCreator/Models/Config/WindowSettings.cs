using System.Runtime.Serialization;
using GalaSoft.MvvmLight;

namespace TestApp.Models.Config
{
    [DataContract(Name = "windowsSettings", Namespace = "")]
    public class StoredWindowSettings
    {
        [DataMember(Name = "visible", Order = 0)]
        public bool IsVisible { get; set; }

        [DataMember(Name = "left", Order = 1)]
        public int Left { get; set; }

        [DataMember(Name = "top", Order = 2)]
        public int Top { get; set; }


        public StoredWindowSettings()
        {
        }

        public StoredWindowSettings(WindowSettings settings)
        {
            IsVisible = settings.IsVisible;
            Left = settings.Left;
            Top = settings.Top;
        }
    }

    
    public class WindowSettings : ViewModelBase
    {
        public bool IsVisible
        {
            get { return _isVisible; }
            set { Set(ref _isVisible, value); }
        }
        private bool _isVisible;

        public int Left
        {
            get { return _left; }
            set { Set(ref _left, value); }
        }
        private int _left;

        public int Top
        {
            get { return _top; }
            set { Set(ref _top, value); }
        }
        private int _top;

        public bool IsSet => Left != 0 && Top != 0;

        public WindowSettings()
        {
            IsVisible = true;
        }

        public WindowSettings(StoredWindowSettings restoredSettings)
        {
            IsVisible = restoredSettings.IsVisible;
            Left = restoredSettings.Left;
            Top = restoredSettings.Top;
        }
    }
}