using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shapes;
using TestApp.Models;
using TestApp.ViewModels.Windows;

namespace TestApp.Views.Windows
{
    public partial class MainWindow
    {
        #region ViewModel

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel",
            typeof (MainViewModel),
            typeof (MainWindow));

        public MainViewModel ViewModel
        {
            get { return (MainViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        #endregion

        private const int WmExitSizeMove = 0x232;

        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
        }

        private void OnLoaded(object sender, RoutedEventArgs args)
        {
            var helper = new WindowInteropHelper(this);
            {
                var source = HwndSource.FromHwnd(helper.Handle);
                source?.AddHook(HwndMessageHook);
            }
            UpdateBackPattern(null, null);
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.MouseDevice.DirectlyOver.GetType() == typeof (Border)) ViewModel.Unselect();
        }

        private IntPtr HwndMessageHook(IntPtr wnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case WmExitSizeMove:
                    UpdateBackPattern(null, null);
                    handled = true;
                    break;
            }
            return IntPtr.Zero;
        }

        void UpdateBackPattern(object sender, SizeChangedEventArgs e)
        {
            return;

            var w = Background.ActualWidth;
            var h = Background.ActualHeight;

            Background.Children.Clear();

            for (var y = 0; y < h; y += Constants.GridWidth)
                for (var x = 0; x < w; x += Constants.GridWidth)
                    AddLineToBackground(x, y, x + 1, y + 1);
        }

        void AddLineToBackground(double x1, double y1, double x2, double y2)
        {
            var line = new Line
            {
                X1 = x1,
                Y1 = y1,
                X2 = x2,
                Y2 = y2,
                Stroke = Brushes.Navy,
                StrokeThickness = 1,
                SnapsToDevicePixels = true
            };
            line.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);
            Background.Children.Add(line);
        }
    }
}