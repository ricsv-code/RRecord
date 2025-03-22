using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace RRecord
{
    public partial class PlaybackWindow : Window
    {
        private MediaPlayerWpfProcess _process;

        private bool _mouseButtonDown = false;
        public PlaybackWindow(MediaPlayerWpfProcess process)
        {
            InitializeComponent();
            _process = process;
            root.DataContext = _process;
            Loaded += PlaybackWindow_Loaded;
        }

        private void PlaybackWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Player.MediaPlayer = _process.MediaPlayer;
        }

        private void ProgressSlider_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _mouseButtonDown = true;
            _process?.SliderDragStarted();
        }

        private void ProgressSlider_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _mouseButtonDown = false;
            _process?.SliderDragCompleted();
        }

        private void ProgressSlider_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Slider slider = sender as Slider;
            if (slider == null)
                return;

            System.Windows.Point pos = e.GetPosition(slider);

            if (_mouseButtonDown == true)
                _process.UpdateVideoPosition(pos.X, slider.ActualWidth);
        }

    }
}
