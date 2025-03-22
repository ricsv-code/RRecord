using System;
using System.IO;
using System.Threading.Tasks;
using Xabe.FFmpeg;

namespace Helpers
{
    public static class FileConverter
    {
        /// <summary>
        /// Konverterar en videofil (MP4 etc) till GIF med hjälp av Xabe.FFmpeg.
        /// </summary>
        /// <param name="inputPath">Sökväg till ursprunglig videofil.</param>
        /// <param name="outputPath">Sökväg till GIF-fil.</param>
        /// <param name="fps">Bildrutor per sekund i GIF.</param>
        public static async Task ConvertToGifAsync(string inputPath, string outputPath, int fps = 10)
        {
            if (!File.Exists(inputPath))
                throw new FileNotFoundException("Video not found", inputPath);


            var conversion = await FFmpeg.Conversions.FromSnippet.ToGif(inputPath, outputPath, fps);


            await conversion.Start();
        }
    }
}
