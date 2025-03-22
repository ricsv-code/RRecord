using LibVLCSharp.Shared;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Utilities;

namespace RRecord
{
    public class MediaPlayerWpfProcess : ViewModelBase, IDisposable
    {
        private LibVLC _libVLC;
        private LibVLCSharp.Shared.MediaPlayer _mediaPlayer;

        private bool _isSliderDragging;
        private double _progress;


        private bool _isPaused;


        private ImageSource _sliderPreviewImage;
        private string _timeDisplay;
        private PlaybackWindow _gui;

        #region Constructor
        public MediaPlayerWpfProcess()
        {
            Core.Initialize();

            _libVLC = new LibVLC();
            _mediaPlayer = new LibVLCSharp.Shared.MediaPlayer(_libVLC);


            _mediaPlayer.EndReached += (sender, args) =>
                System.Threading.ThreadPool.QueueUserWorkItem(_ =>
                {
                    _mediaPlayer.Stop();

                    System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        Progress = 100;
                    }));
                });

            PlayCommand = new RelayCommand(Play);
            PauseCommand = new RelayCommand(Pause);
            OpenCommand = new RelayCommand(Open);



            _gui = new PlaybackWindow(this);
            _gui.Show();

            CompositionTarget.Rendering -= CompositionTarget_Rendering;
            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        #endregion

        #region Events

        #endregion

        #region Properties

        public LibVLCSharp.Shared.MediaPlayer MediaPlayer => _mediaPlayer;

        public long TotalTime => _mediaPlayer.Length;


        public ICommand PlayCommand { get; }
        public ICommand PauseCommand { get; }
        public ICommand OpenCommand { get; }

        public double Progress
        {
            get => _progress;
            set
            {
                if (SetProperty(ref _progress, value))
                {
                    long total = _mediaPlayer.Length;
                    if (_isSliderDragging && total > 0)
                    {
                        long newTime = (long)(_progress / 100 * total);
                        _mediaPlayer.Time = newTime;
                    }
                }
            }
        }

        public string TimeDisplay
        {
            get => _timeDisplay;
            set => SetProperty(ref _timeDisplay, value);
        }


        public ImageSource SliderPreviewImage
        {
            get => _sliderPreviewImage;
            set => SetProperty(ref _sliderPreviewImage, value);
        }


        #endregion


        #region Methods
        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {

            if (_mediaPlayer == null || _mediaPlayer.Length == 0 || _isSliderDragging)
                return;

            long currentTime = _mediaPlayer.Time;
            long totalTime = _mediaPlayer.Length;


            Progress = (double)currentTime / totalTime * 100;
            TimeSpan elapsed = TimeSpan.FromMilliseconds(currentTime);
            TimeSpan total = TimeSpan.FromMilliseconds(totalTime);
            TimeDisplay = $"{elapsed:mm\\:ss} / {total:mm\\:ss}";

        }


        public void StartMedia(string filePath)
        {
            if (File.Exists(filePath))
            {
                _mediaPlayer.Stop();

                _isPaused = (bool)_mediaPlayer.Play(new Media(_libVLC, new Uri(filePath)));

            }
            else
            {
                System.Windows.MessageBox.Show("File not found: " + filePath, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }




        public void Play(object sender)
        {

            if (_mediaPlayer.State == VLCState.Ended)
            {
                _mediaPlayer.Time = 0;
            }

            _isPaused = (bool)_mediaPlayer?.Play();

        }

        public void Pause(object sender)
        {
            _mediaPlayer?.Pause();

            _isPaused = !_isPaused;

        }

        public void Open(object sender)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Open Video File",
                Filter = "Video Files|*.mp4;*.avi;*.mkv;*.mov;*.wmv|All Files|*.*",
                InitialDirectory = Path.GetDirectoryName(Helpers.TempFileManager.GetTempVideoFilePath())
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string newFilePath = openFileDialog.FileName;
                if (File.Exists(newFilePath))
                {

                    StartMedia(newFilePath);
                }
                else
                {
                    System.Windows.MessageBox.Show("File not found: " + newFilePath, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool _wasPlaying;

        public void SliderDragStarted()
        {
            _isSliderDragging = true;


            if (_wasPlaying = _mediaPlayer.State == VLCState.Playing)
            {
                _mediaPlayer.Pause();
                _isPaused = true;
            }
            

        }

        public void SliderDragCompleted()
        {
            _isSliderDragging = false;

            long total = _mediaPlayer.Length;
            if (_mediaPlayer != null && total > 0)
            {
                long newTime = (long)(Progress / 100 * total);
                _mediaPlayer.Time = newTime;
            }


            if (_wasPlaying)
            {
                _mediaPlayer?.Play();
                _isPaused = false;
            }
        }



        public void UpdateVideoPosition(double mouseX, double sliderWidth)
        {
            if (sliderWidth <= 0)
                return;

            double fraction = mouseX / sliderWidth;
            if (fraction < 0) fraction = 0;
            if (fraction > 1) fraction = 1;

            long totalTime = _mediaPlayer.Length;
            if (totalTime <= 0)
                return;

            long newTime = (long)(fraction * totalTime);
            _mediaPlayer.Time = newTime;

        }



        public void Dispose()
        {
            CompositionTarget.Rendering -= CompositionTarget_Rendering;
            _gui.Close();
            _mediaPlayer?.Stop();
            _mediaPlayer?.Dispose();
            _libVLC?.Dispose();
        }

        #endregion
    }
}
