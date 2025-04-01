using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

namespace RRecord
{
    public partial class CaptureWindow : Window
    {
        private const double AnchorWidth = 24;
        private const double AnchorHeight = 24;
        private const double MinSize = 100;

        private bool _isMovingAnchor = false;
        private bool _isResizing = false;
        private ResizeZone _zone = ResizeZone.None;

        private double _startLeft, _startTop, _startWidth, _startHeight;

        private System.Windows.Point _startMousePosScreen;

        public CaptureWindow(Rect region)
        {
            InitializeComponent();
            this.Left = region.Left;
            this.Top = region.Top;
            this.Width = region.Width;
            this.Height = region.Height;
        }

        public event EventHandler<Rect> CaptureWindowResized;

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            double w = this.ActualWidth;
            double h = this.ActualHeight;

            Canvas.SetLeft(HitRect, 0);
            Canvas.SetTop(HitRect, 0);
            HitRect.Width = w;
            HitRect.Height = h;

            Canvas.SetLeft(BorderRect, 0);
            Canvas.SetTop(BorderRect, 0);
            BorderRect.Width = w;
            BorderRect.Height = h;

            Canvas.SetLeft(AnchorRect, 0);
            Canvas.SetTop(AnchorRect, 0);

            CaptureWindowResized?.Invoke(this, new Rect (this.Left, this.Top, this.Width, this.Height));
        }

        private void Window_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            _startLeft = this.Left;
            _startTop = this.Top;
            _startWidth = this.Width;
            _startHeight = this.Height;
            var pos = System.Windows.Forms.Cursor.Position; 
            _startMousePosScreen = new System.Windows.Point(pos.X, pos.Y);

            System.Windows.Point pInWindow = e.GetPosition(this);

            if (IsInAnchorBox(pInWindow))
            {
                _isMovingAnchor = true;
                this.CaptureMouse();
                return;
            }

            _zone = DetermineZone(pInWindow);
            if (_zone != ResizeZone.None)
            {
                _isResizing = true;
                this.CaptureMouse();
                return;
            }
        }

        private void Window_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (_isMovingAnchor)
            {
                var pos = System.Windows.Forms.Cursor.Position;
                double currentX = pos.X;
                double currentY = pos.Y;
                double dx = currentX - _startMousePosScreen.X;
                double dy = currentY - _startMousePosScreen.Y;
                this.Left = _startLeft + dx;
                this.Top = _startTop + dy;
            }
            else if (_isResizing)
            {
                ResizeWindow(_zone);
            }
            else
            {
                System.Windows.Point pInWindow = e.GetPosition(this);
                if (IsInAnchorBox(pInWindow))
                {
                    this.Cursor = System.Windows.Input.Cursors.Arrow;
                }
                else
                {
                    var z = DetermineZone(pInWindow);
                    this.Cursor = CursorForZone(z);
                }
            }
        }

        private void Window_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isMovingAnchor = false;
            _isResizing = false;
            _zone = ResizeZone.None;
            this.Cursor = System.Windows.Input.Cursors.Arrow;
            Mouse.Capture(null);
        }

        private bool IsInAnchorBox(System.Windows.Point p)
        {
            return (p.X >= 0 && p.X <= AnchorWidth &&
                    p.Y >= 0 && p.Y <= AnchorHeight);
        }

        private ResizeZone DetermineZone(System.Windows.Point p)
        {
            double w = this.ActualWidth;
            double h = this.ActualHeight;
            double border = 15; 

            bool onLeft = (p.X <= border);
            bool onRight = (p.X >= w - border);
            bool onTop = (p.Y <= border);
            bool onBottom = (p.Y >= h - border);

            if (onTop && onLeft) return ResizeZone.TopLeft;
            if (onTop && onRight) return ResizeZone.TopRight;
            if (onBottom && onLeft) return ResizeZone.BottomLeft;
            if (onBottom && onRight) return ResizeZone.BottomRight;
            if (onTop) return ResizeZone.Top;
            if (onBottom) return ResizeZone.Bottom;
            if (onLeft) return ResizeZone.Left;
            if (onRight) return ResizeZone.Right;

            return ResizeZone.None;
        }

        private System.Windows.Input.Cursor CursorForZone(ResizeZone z)
        {
            switch (z)
            {
                case ResizeZone.Top:
                case ResizeZone.Bottom: return System.Windows.Input.Cursors.SizeNS;
                case ResizeZone.Left:
                case ResizeZone.Right: return System.Windows.Input.Cursors.SizeWE;
                case ResizeZone.TopLeft:
                case ResizeZone.BottomRight: return System.Windows.Input.Cursors.SizeNWSE;
                case ResizeZone.TopRight:
                case ResizeZone.BottomLeft: return System.Windows.Input.Cursors.SizeNESW;
                default: return System.Windows.Input.Cursors.Arrow;
            }
        }


        private void ResizeWindow(ResizeZone zone)
        {
            var pos = System.Windows.Forms.Cursor.Position;
            double currentX = pos.X;
            double currentY = pos.Y;

            double newLeft = _startLeft;
            double newTop = _startTop;
            double newWidth = _startWidth;
            double newHeight = _startHeight;

            switch (zone)
            {
                case ResizeZone.Left:
                    newLeft = currentX;
                    newWidth = _startWidth + (_startLeft - currentX);
                    break;
                case ResizeZone.Right:
                    newWidth = currentX - _startLeft;
                    break;
                case ResizeZone.Top:
                    newTop = currentY;
                    newHeight = _startHeight + (_startTop - currentY);
                    break;
                case ResizeZone.Bottom:
                    newHeight = currentY - _startTop;
                    break;
                case ResizeZone.TopLeft:
                    newLeft = currentX;
                    newWidth = _startWidth + (_startLeft - currentX);
                    newTop = currentY;
                    newHeight = _startHeight + (_startTop - currentY);
                    break;
                case ResizeZone.TopRight:
                    newWidth = currentX - _startLeft;
                    newTop = currentY;
                    newHeight = _startHeight + (_startTop - currentY);
                    break;
                case ResizeZone.BottomLeft:
                    newLeft = currentX;
                    newWidth = _startWidth + (_startLeft - currentX);
                    newHeight = currentY - _startTop;
                    break;
                case ResizeZone.BottomRight:
                    newWidth = currentX - _startLeft;
                    newHeight = currentY - _startTop;
                    break;
            }


            if (newWidth < MinSize)
            {
                if (zone == ResizeZone.Left || zone == ResizeZone.TopLeft || zone == ResizeZone.BottomLeft)
                    newLeft = _startLeft + (_startWidth - MinSize);
                newWidth = MinSize;
            }
            if (newHeight < MinSize)
            {
                if (zone == ResizeZone.Top || zone == ResizeZone.TopLeft || zone == ResizeZone.TopRight)
                    newTop = _startTop + (_startHeight - MinSize);
                newHeight = MinSize;
            }

            this.Left = newLeft;
            this.Top = newTop;
            this.Width = newWidth;
            this.Height = newHeight;
        }

        public Rect GetSelectedRegionOnScreen()
        {
            return new Rect(this.Left + 5, this.Top + 5, this.ActualWidth - 10, this.ActualHeight - 10);
        }
    }

    public enum ResizeZone
    {
        None,
        Top, Bottom, Left, Right,
        TopLeft, TopRight, BottomLeft, BottomRight
    }
}
