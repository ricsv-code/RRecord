using Helpers;
using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Utilities;
using System.Windows;
using Microsoft.Win32;
using System.Diagnostics;
using Newtonsoft.Json.Linq;

namespace RRecord
{
    public class RRecordWpfProcess : ViewModelBase
    {
        #region Fields

        private ScreenRecorder _screenRecorder;

        private CaptureWindow _captureWindow;

        private MediaPlayerWpfProcess _mediaPlayerWpfProcess;

        private SnipWindow _snipWindow;

        private MainWindow _gui;

        private bool _isRecording;


        #endregion

        #region Constructors
        public RRecordWpfProcess()
        {
            _gui = new MainWindow(this);
            _screenRecorder = new ScreenRecorder();

            //settings för ljud osv


            CreateCommand = new RelayCommand(Create);
            RecordCommand = new RelayCommand(Record);
            StopCommand = new RelayCommand(Stop);
            PlayCommand = new RelayCommand(Play);
            SaveVidCommand = new RelayCommand(SaveVid);
            SaveGifCommand = new RelayCommand(SaveGif);
            AudioDeviceCommand = new RelayCommand(AudioDevice);



            RecordButtonEnabled = false;
            StopButtonEnabled = false;
            PlayButtonEnabled = true;


            _gui.WindowState = WindowState.Minimized;
            _gui.Visibility = Visibility.Collapsed;

  
        }

        #endregion

        #region Events

        // CaptureWindow resize event
        public event EventHandler<Rect> RegionChanged;

        #endregion


        #region Properties
               



        // Button Enable/Disable
        private bool _createButtonEnabled;
        public bool CreateButtonEnabled
        {
            get => _createButtonEnabled;
            set => SetProperty(ref _createButtonEnabled, value);
        }

        private bool _recordButtonEnabled;
        public bool RecordButtonEnabled
        {
            get => _recordButtonEnabled;
            set => SetProperty(ref _recordButtonEnabled, value);
        }

        private bool _stopButtonEnabled;
        public bool StopButtonEnabled
        {
            get => _stopButtonEnabled;
            set => SetProperty(ref _stopButtonEnabled, value);
        }

        private bool _playButtonEnabled;
        public bool PlayButtonEnabled
        {
            get => _playButtonEnabled;
            set => SetProperty(ref _playButtonEnabled, value);
        }

        private bool _saveVideoButtonEnabled;
        public bool SaveVideoButtonEnabled
        {
            get => _saveVideoButtonEnabled;
            set => SetProperty(ref _saveVideoButtonEnabled, value);
        }

        private bool _saveGifButtonEnabled;
        public bool SaveGifButtonEnabled
        {
            get => _saveGifButtonEnabled;
            set => SetProperty(ref _saveGifButtonEnabled, value);
        }


        // Indicators
        private Color _recordIndicatorFill;

        public Color RecordIndicatorFill
        {
            get => _recordIndicatorFill;
            set => SetProperty(ref _recordIndicatorFill, value);
        }

        private string _statusText;
        public string StatusText
        {
            get => _statusText;
            set => SetProperty(ref _statusText, value);
        }

        // Commands
        public ICommand CreateCommand { get; }
        public ICommand RecordCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand PlayCommand { get; }
        public ICommand SaveVidCommand { get; }
        public ICommand SaveGifCommand { get; }
        public ICommand AudioDeviceCommand { get; }

        #endregion


        #region Methods

        public void Create(object sender)
        {

            if (_captureWindow != null)
            {
                _captureWindow.Close();
            }

            if (_snipWindow == null)
                _snipWindow = new SnipWindow();

            bool? result = _snipWindow.ShowDialog();

            if (result == true)
            {
                var region = _snipWindow.Selection;
                if (region.Width > 0 && region.Height > 0)
                {

                    _captureWindow = new CaptureWindow(region);
                    _captureWindow.Show();

                    _captureWindow.CaptureWindowResized -= (s, e) => RegionChanged?.Invoke(s, e);
                    _captureWindow.CaptureWindowResized += (s, e) => RegionChanged?.Invoke(s, e);

                    OpenGui();

                    RegionChanged?.Invoke(this, region);                    

                    RecordButtonEnabled = true;
                }
            }
            else
            {
                RecordButtonEnabled = false;
            }

        }

        private async void Record(object sender)
        {
            if (_captureWindow == null)
            {
                System.Windows.MessageBox.Show("No capture window found");
                return;
            }

            var region = _captureWindow.GetSelectedRegionOnScreen();

            _gui.StopBtn.IsEnabled = true;

            _screenRecorder.StartRecording(
                (int)region.X, (int)region.Y,
                (int)region.Width, (int)region.Height);

            _isRecording = true;
            await IndicatorGoAsync();
        }

        private async Task IndicatorGoAsync()
        {
            if (_isRecording)
            {
                StatusText = "Recording...";
            }

            while (_isRecording)
            {
                RecordIndicatorFill = Color.Red;
                await Task.Delay(500);
                RecordIndicatorFill = Color.Transparent;
                await Task.Delay(500);
            }
        } 
        

        private void Stop(object sender)
        {
            _screenRecorder.StopRecording();
            _isRecording = false;

            _gui.RecordBtn.IsEnabled = true;
            _gui.StopBtn.IsEnabled = false;
            _gui.PlayBtn.IsEnabled = true;
        }

        private void Play(object sender)
        {

            string tempPath = TempFileManager.GetTempVideoFilePath();


            if (tempPath != null && System.IO.File.Exists(tempPath))
            {
                if (_mediaPlayerWpfProcess == null)
                    _mediaPlayerWpfProcess = new MediaPlayerWpfProcess();

                _mediaPlayerWpfProcess.Open(this);
                //_mediaPlayerWpfProcess.StartMedia(tempPath);
            }
            else
            {
                _mediaPlayerWpfProcess = new MediaPlayerWpfProcess();
                _mediaPlayerWpfProcess.Open(this);
            }
        }

        private async void SaveVid(object sender)
        {

            Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "MP4 Video|*.mp4",
                FileName = "MyCapture.mp4"
            };
            if (sfd.ShowDialog() == true)
            {
                string tempPath = TempFileManager.GetTempVideoFilePath();
                if (!System.IO.File.Exists(tempPath))
                {
                    System.Windows.MessageBox.Show("SaveVid: No file found.");
                    return;
                }

                try
                {
                    System.IO.File.Copy(tempPath, sfd.FileName, true);

                    TempFileManager.DeleteTempVideo();
                    System.Windows.MessageBox.Show("Video saved!");
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show("Couldn't save video: " + ex.Message);
                }
            }
        }

        private async void SaveGif(object sender)
        {
            Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "GIF Image|*.gif",
                FileName = "MyCapture.gif"
            };
            if (sfd.ShowDialog() == true)
            {
                string tempPath = TempFileManager.GetTempVideoFilePath();
                if (!System.IO.File.Exists(tempPath))
                {
                    System.Windows.MessageBox.Show("SaveGif: No file found.");
                    return;
                }

                try
                {

                    await FileConverter.ConvertToGifAsync(tempPath, sfd.FileName, 10);

                    TempFileManager.DeleteTempVideo();
                    System.Windows.MessageBox.Show("GIF saved!");
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show("Couldn't save gif: " + ex.Message);
                }
            }
        }


        public void CloseGui()
        {
            _gui.Close();
        }

        public void OpenGui()
        {

            if (_gui != null)
            {
                if (_gui.WindowState == WindowState.Minimized)
                {
                    _gui.WindowState = WindowState.Normal;
                }
                _gui.Show();
                _gui.Activate();
            }
        }


        private void AudioDevice(object sender)
        {



        }


        #endregion
    }
}
