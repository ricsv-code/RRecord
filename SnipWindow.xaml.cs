using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace RRecord
{
    public partial class SnipWindow : Window
    {
        private System.Windows.Point _start;
        private bool _isSelecting;
        private Rect _selection;


        public Rect Selection => _selection;

        public SnipWindow()
        {
            InitializeComponent();

            this.Left = SystemParameters.VirtualScreenLeft;
            this.Top = SystemParameters.VirtualScreenTop;
            this.Width = SystemParameters.VirtualScreenWidth;
            this.Height = SystemParameters.VirtualScreenHeight;
        }

        private void Window_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.DialogResult = false;
                this.Close();
            }
        }

        private void SnipCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isSelecting = true;
            _start = e.GetPosition(SnipCanvas);

            Canvas.SetLeft(SelectionRect, _start.X);
            Canvas.SetTop(SelectionRect, _start.Y);
            SelectionRect.Width = 0;
            SelectionRect.Height = 0;
            SelectionRect.Visibility = Visibility.Visible;
        }


        private void SnipCanvas_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (_isSelecting)
            {
                System.Windows.Point currentPos = e.GetPosition(SnipCanvas);

                double x = Math.Min(currentPos.X, _start.X);
                double y = Math.Min(currentPos.Y, _start.Y);
                double w = Math.Abs(currentPos.X - _start.X);
                double h = Math.Abs(currentPos.Y - _start.Y);

                Canvas.SetLeft(SelectionRect, x);
                Canvas.SetTop(SelectionRect, y);
                SelectionRect.Width = w;
                SelectionRect.Height = h;
            }
        }


        private void SnipCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isSelecting = false;

            double left = Canvas.GetLeft(SelectionRect);
            double top = Canvas.GetTop(SelectionRect);
            double width = SelectionRect.Width;
            double height = SelectionRect.Height;

            System.Windows.Point screenPos = this.PointToScreen(new System.Windows.Point(left, top));
            _selection = new Rect(screenPos.X, screenPos.Y, width, height);

            this.DialogResult = true;
            this.Close();
        }


        private void SnipCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            _selection = Rect.Empty; 
            this.DialogResult = false;
            this.Close();
        }
    }
}
