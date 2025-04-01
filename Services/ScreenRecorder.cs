using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Helpers;
using System.Text;

namespace Services
{


    public class ScreenRecorder
    {
        private Process _ffmpegProcess;

        /// <summary>
        /// Startar skärminspelning med FFmpeg, med valfritt ljud.
        /// </summary>
        /// <param name="x">Offset X.</param>
        /// <param name="y">Offset Y.</param>
        /// <param name="width"/>
        /// <param name="height"/>
        /// <param name="audioDeviceName">Namn på ljudenhet. Om null, inspelas ingen ljud.</param>
        public void StartRecording(int x, int y, int width, int height, string audioDeviceName = null)
        {

            string tempVideoPath = TempFileManager.GetTempVideoFilePath();

            var argsBuilder = new StringBuilder();
            argsBuilder.Append("-y "); // yes till att overwritea output

            argsBuilder.Append($"-f gdigrab -framerate 30 -offset_x {x} -offset_y {y} ");
            argsBuilder.Append($"-video_size {width}x{height} -i desktop ");

            if (!string.IsNullOrEmpty(audioDeviceName))
            {
                argsBuilder.Append($"-f dshow -i \"{audioDeviceName}\" ");
            }

            argsBuilder.Append("-c:v libx264 -preset veryfast -crf 23 ");

            if (!string.IsNullOrEmpty(audioDeviceName))
            {
                argsBuilder.Append("-c:a aac ");
            }

            argsBuilder.Append("-movflags +faststart ");

            argsBuilder.Append($"\"{tempVideoPath}\"");

            string args = argsBuilder.ToString();


            string ffmpegPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg.exe");
            if (!File.Exists(ffmpegPath))
            {
                System.Windows.MessageBox.Show($"FFmpeg executable not found at {ffmpegPath}");
                return;
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = args,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardInput = true
            };

            _ffmpegProcess = new Process { StartInfo = startInfo, EnableRaisingEvents = true };

            _ffmpegProcess.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    Console.WriteLine("FFmpeg: " + e.Data);
                }
            };

            try
            {
                _ffmpegProcess.Start();
                _ffmpegProcess.BeginErrorReadLine();
                System.Windows.MessageBox.Show($"Inspelning startad till {tempVideoPath}");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Fel vid start av FFmpeg: {ex.Message}");
            }
        }

        /// <summary>
        /// Stoppar inspelningen genom att skicka "q" till FFmpeg-processen eller avsluta den.
        /// </summary>
        public void StopRecording()
        {
            try
            {
                if (_ffmpegProcess != null && !_ffmpegProcess.HasExited)
                {
                    _ffmpegProcess.StandardInput.Write("q\n");
                    _ffmpegProcess.StandardInput.Flush();

                    System.Windows.MessageBox.Show("Stoppar inspelningen...");
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Fel vid stopp av inspelning: {ex.Message}");
            }
            finally
            {
                _ffmpegProcess?.WaitForExit(2000);
                if (_ffmpegProcess != null && !_ffmpegProcess.HasExited)
                {
                    _ffmpegProcess.Kill();
                }
                _ffmpegProcess?.Dispose();
                _ffmpegProcess = null;

                System.Windows.MessageBox.Show("Inspelning stoppad.");
            }
        }
    }

}
