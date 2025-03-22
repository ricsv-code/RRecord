using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

public class FFmpegDeviceManager
{
    /// <summary>
    /// Hämtar en lista över tillgängliga DirectShow-ljudenheter via FFmpeg.
    /// </summary>
    /// <returns>Lista med ljudenhetsnamn.</returns>
    public List<string> GetAudioDevices()
    {
        List<string> audioDevices = new List<string>();

        string ffmpegPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg.exe");
        if (!File.Exists(ffmpegPath))
        {
            throw new FileNotFoundException($"FFmpeg executable not found at {ffmpegPath}");
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = ffmpegPath,
            Arguments = "-list_devices true -f dshow -i dummy",
            UseShellExecute = false,
            RedirectStandardError = true,
            RedirectStandardOutput = false,
            CreateNoWindow = true
        };

        using (var process = new Process { StartInfo = startInfo })
        {
            process.Start();

            string stderr = process.StandardError.ReadToEnd();
            process.WaitForExit();

            bool isAudioSection = false;
            foreach (var line in stderr.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (line.Contains("DirectShow audio devices"))
                {
                    isAudioSection = true;
                    continue;
                }

                if (isAudioSection)
                {
                    var match = Regex.Match(line, @"""(.*)""");
                    if (match.Success)
                    {
                        audioDevices.Add(match.Groups[1].Value);
                    }
                    else
                    {
                        if (line.Contains("DirectShow video devices") || line.Contains("Alternative name") || string.IsNullOrWhiteSpace(line))
                        {
                            break;
                        }
                    }
                }
            }
        }

        return audioDevices;
    }
}
