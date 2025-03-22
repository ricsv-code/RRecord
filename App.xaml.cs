using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Forms;
using Xabe.FFmpeg.Downloader;
using Xabe.FFmpeg;

namespace RRecord
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private NotifyIcon _notifyIcon;
        private RRecordWpfProcess _process;
        private HotKeyManager _hotKeyManager;

        private void ApplicationStartup(object sender, StartupEventArgs e)
        {

            DownloadFfmpeg();

            SetupTrayIcon();

            ListenForHotkey();

            Helpers.SettingsManager.SettingsChanged += (sender, e) => ListenForHotkey();

        }

        private void ListenForHotkey()
        {
            var modifiers = (HotKeyManager.ModifierKeys)Helpers.SettingsManager.CurrentSettings.HotKeyModifiers;
            var key = (uint)Helpers.SettingsManager.CurrentSettings.HotKeyKey;

            if (_hotKeyManager != null)
            {
                _hotKeyManager.HotKeyPressed -= HotkeyPressed;
                _hotKeyManager.Dispose();
            }

            _hotKeyManager = new HotKeyManager(modifiers, key);

            _hotKeyManager.HotKeyPressed -= HotkeyPressed; //gör ingenting?
            _hotKeyManager.HotKeyPressed += HotkeyPressed;
        }

        private void HotkeyPressed(object? sender, EventArgs e)
        {
            if (_process == null)
                _process = new RRecordWpfProcess();

            _process.Create(this);
        }

        private void SetupTrayIcon()
        {
            _notifyIcon = new NotifyIcon
            {
                Icon = SystemIcons.Application,
                Visible = true,
                Text = "RRecord"
            };

            var contextMenu = new System.Windows.Forms.ContextMenuStrip();
            contextMenu.Items.Add("Open", null, Open_Click);
            contextMenu.Items.Add("Exit", null, Exit_Click);
            _notifyIcon.ContextMenuStrip = contextMenu;

            _notifyIcon.DoubleClick -= Open_Click;
            _notifyIcon.DoubleClick += Open_Click;
        }


        private void Open_Click(object? sender, EventArgs e)
        {
            if (_process == null)
                _process = new RRecordWpfProcess();

            _process.OpenGui();

        }

        private void Exit_Click(object? sender, EventArgs e)
        {
            if (_process != null)
                _process.CloseGui();

            _notifyIcon.Dispose();
            Shutdown();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _notifyIcon.Dispose();
            base.OnExit(e);
        }

        private async void DownloadFfmpeg()
        {
            var ffmpegDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg");

            await FFmpegDownloader.GetLatestVersion(FFmpegVersion.Official);

            FFmpeg.SetExecutablesPath(ffmpegDir);
        }
    }

}
