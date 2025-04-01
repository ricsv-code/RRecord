using System;
using System.IO;
using System.Threading.Tasks;
using Xabe.FFmpeg;

namespace Helpers
{
    public static class FileConverter
    {

        public static async Task ConvertToGifAsync(string inputPath, string outputPath, int fps = 10)
        {
            if (!File.Exists(inputPath))
                throw new FileNotFoundException("Video not found", inputPath);


            var conversion = await FFmpeg.Conversions.FromSnippet.ToGif(inputPath, outputPath, fps);


            await conversion.Start();
        }
    }
}
