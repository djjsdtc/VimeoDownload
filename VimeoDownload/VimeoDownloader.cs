namespace VimeoDownload
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using VimeoDownload.DataContract;

    public class VimeoDownloader
    {
        private List<string> videoTempFileNames;

        private List<string> audioTempFileNames;

        public string DownloadAddress { get; set; }

        public string OutputFilename { get; set; }

        public async Task DownloadVideo()
        {
            using (var httpClient = new HttpClient())
            {
                Console.WriteLine("Downloading metadata...");
                var videoInfo = await WebUtility.GetVideoInfo(httpClient, this.DownloadAddress);
                Console.WriteLine($"The video has {videoInfo.Video.Count} video clip(s) and {videoInfo.Audio} audio clip(s).");
                var tempDir = Directory.CreateDirectory(videoInfo.ClipId);
                Console.WriteLine($"Created directory {tempDir.FullName} to store temporary segments.");
                Parallel.For(0, videoInfo.Video.Count,
                    new ParallelOptions { MaxDegreeOfParallelism = 1 },
                    async i =>
                {
                    await DownloadMediaClip(httpClient, videoInfo.Video[i], Path.Combine(tempDir.FullName, $"video{i}.m4v"), videoInfo.IsBase64Init);
                });
            }
        }

        private async Task DownloadMediaClip(HttpClient httpClient, MediaClip clipData, string outputFile, bool isBase64Init)
        {
            using (var fileStream = File.Create(outputFile))
            {
                var fsOffset = 0;
                if (isBase64Init)
                {
                    var initSegment = Convert.FromBase64String(clipData.InitSegment);
                    fileStream.Write(initSegment, fsOffset, initSegment.Length);
                    foreach(var segment in clipData.Segments)
                    {
                        
                    }
                }
            }
        }
    }
}