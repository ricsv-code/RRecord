using System;
using System.IO;

namespace Helpers
{
    public static class TempFileManager
    {
        private static string _tempVideoFilePath;

        /// <summary>
        /// Genererar en temporär filväg för videoinspelning.
        /// </summary>
        public static string GetTempVideoFilePath()
        {
            if (string.IsNullOrEmpty(_tempVideoFilePath))
            {
                string fileName = $"capture_{Guid.NewGuid()}.mp4";


                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string tempDirectory = Path.Combine(baseDirectory, "Temp");

                if (!Directory.Exists(tempDirectory))
                {
                    Directory.CreateDirectory(tempDirectory);
                }

                _tempVideoFilePath = Path.Combine(tempDirectory, fileName);
            }
            return _tempVideoFilePath;
        }

        /// <summary>
        /// Raderar temporärvideon om den finns.
        /// </summary>
        public static void DeleteTempVideo()
        {
            if (!string.IsNullOrEmpty(_tempVideoFilePath) && File.Exists(_tempVideoFilePath))
            {
                File.Delete(_tempVideoFilePath);
            }
            _tempVideoFilePath = null;
        }
    }
}
