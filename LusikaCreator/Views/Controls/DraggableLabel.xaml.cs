using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using TestApp.Models;
using TestApp.ViewModels.Controls;

namespace TestApp.Views.Controls
{
    public partial class DraggableLabel : UserControl
    {
        private Vector _relativeMousePos; // смещение мыши от левого верхнего угла квадрата
        private Canvas _container;        // канвас-контейнер

        #region ViewModel

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel",
            typeof (LabelViewModel),
            typeof (DraggableLabel));

        public LabelViewModel ViewModel
        {
            get { return (LabelViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        #endregion

        public DraggableLabel()
        {
            InitializeComponent();
            SetBinding(RequestMoveCommandProperty, new Binding("Properties.RequestMove"));
        }

        public ICommand RequestMoveCommand
        {
            get { return (ICommand) GetValue(RequestMoveCommandProperty); }
            set { SetValue(RequestMoveCommandProperty, value); }
        }

        public static readonly DependencyProperty RequestMoveCommandProperty =
            DependencyProperty.Register("RequestMoveCommand", typeof(ICommand),
                typeof(DraggableLabel));

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!ViewModel.Properties.IsInEditMode) return;
            ViewModel.Properties.Select();

            _container = Common.FindParent<Canvas>(this);
            _relativeMousePos = e.GetPosition(this) - new Point();
            MouseMove += OnDragMove;
            LostMouseCapture += OnLostCapture;
            Mouse.Capture(this);
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            FinishDrag(sender, e);
            Mouse.Capture(null);
        }

        private void OnLostCapture(object sender, MouseEventArgs e)
        {
            FinishDrag(sender, e);
        }

        void OnDragMove(object sender, MouseEventArgs e)
        {
            UpdatePosition(e);
        }


        void FinishDrag(object sender, MouseEventArgs e)
        {
            MouseMove -= OnDragMove;
            LostMouseCapture -= OnLostCapture;
            UpdatePosition(e);
        }

        private void MoveTo(Point point, bool useGrid)
        {
            var radius = Constants.GridWidth;
            var currentPoint = point - _relativeMousePos;

            var posX = (int) currentPoint.X;
            var posY = (int) currentPoint.Y;

            if (useGrid)
            {
                if (posX%radius != 0) currentPoint.X = posX/radius*radius;
                if (posY%radius != 0) currentPoint.Y = posY/radius*radius;
            }

            if (currentPoint.X < radius) currentPoint.X = radius;
            if (currentPoint.Y < radius) currentPoint.Y = radius;

            RequestMoveCommand?.Execute(currentPoint);
        }

        private void UpdatePosition(MouseEventArgs e)
        {
            if (!ViewModel.Properties.IsInEditMode) return;

            var point = e.GetPosition(_container);
            MoveTo(point, true);
        }
    }
}