using System;
using System.Windows;
using System.Windows.Forms; 

namespace RRecord
{
    public partial class MainWindow : Window
    {

        public MainWindow(RRecordWpfProcess process)
        {
            InitializeComponent();
            root.DataContext = process;

            process.RegionChanged += (sender, newRegion) =>
            {
                this.Dispatcher.Invoke(() => this.Position(newRegion));
            };

        }


        public void Position(Rect region)
        {
            const double margin = 10;

            var regionRect = new System.Drawing.Rectangle((int)region.X, (int)region.Y, (int)region.Width, (int)region.Height);
            Screen screen = Screen.FromRectangle(regionRect);
            var wa = screen.WorkingArea; 

            double workingLeft = wa.Left;
            double workingTop = wa.Top;
            double workingRight = wa.Right;
            double workingBottom = wa.Bottom;

            double windowWidth = this.ActualWidth; 
            double windowHeight = this.ActualHeight;


            if (region.Bottom + margin + windowHeight <= workingBottom)
            {
                double desiredLeft = region.Left + (region.Width - windowWidth) / 2;

                desiredLeft = Math.Max(workingLeft, Math.Min(desiredLeft, workingRight - windowWidth));
                this.Left = desiredLeft;
                this.Top = region.Bottom + margin;
                return;
            }


            if (region.Top - margin - windowHeight >= workingTop)
            {
                double desiredLeft = region.Left + (region.Width - windowWidth) / 2;
                desiredLeft = Math.Max(workingLeft, Math.Min(desiredLeft, workingRight - windowWidth));
                this.Left = desiredLeft;
                this.Top = region.Top - margin - windowHeight;
                return;
            }

            if (region.Right + margin + windowWidth <= workingRight)
            {
                double desiredTop = region.Top + (region.Height - windowHeight) / 2;
                desiredTop = Math.Max(workingTop, Math.Min(desiredTop, workingBottom - windowHeight));
                this.Left = region.Right + margin;
                this.Top = desiredTop;
                return;
            }

            if (region.Left - margin - windowWidth >= workingLeft)
            {
                double desiredTop = region.Top + (region.Height - windowHeight) / 2;
                desiredTop = Math.Max(workingTop, Math.Min(desiredTop, workingBottom - windowHeight));
                this.Left = region.Left - margin - windowWidth;
                this.Top = desiredTop;
                return;
            }

            this.Left = workingLeft + (wa.Width - windowWidth) / 2;
            this.Top = workingTop + (wa.Height - windowHeight) / 2;
        }
    }
}
