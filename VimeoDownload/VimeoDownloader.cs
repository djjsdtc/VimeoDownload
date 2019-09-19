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
        public string DownloadAddress { get; set; }

        public string OutputFilename { get; set; }

        public async Task DownloadVideo()
        {
            using (var httpClient = new HttpClient())
            {
                Console.WriteLine("Downloading metadata...");
                var videoInfo = await WebUtility.GetVideoInfo(httpClient, this.DownloadAddress);
                Console.WriteLine($"The video has {videoInfo.Video.Count} video clip(s) and {videoInfo.Audio.Count} audio clip(s).");
                var tempDir = Directory.CreateDirectory(videoInfo.ClipId);
                Console.WriteLine($"Created directory {tempDir.FullName} to store temporary segments.");
                var baseUrl = WebUtility.CombimeUrl(this.DownloadAddress, videoInfo.BaseUrl);
                Parallel.For(0, videoInfo.Video.Count,
                    new ParallelOptions { MaxDegreeOfParallelism = 1 },
                    i =>
                    {
                        DownloadMediaClip(httpClient, videoInfo.Video[i], baseUrl, Path.Combine(tempDir.FullName, $"video{i}.m4v"), videoInfo.IsBase64Init).Wait();
                    });
                Parallel.For(0, videoInfo.Audio.Count,
                    new ParallelOptions { MaxDegreeOfParallelism = 1 },
                    i =>
                    {
                        DownloadMediaClip(httpClient, videoInfo.Audio[i], baseUrl, Path.Combine(tempDir.FullName, $"audio{i}.m4a"), videoInfo.IsBase64Init).Wait();
                    });
            }
        }

        private async Task DownloadMediaClip(HttpClient httpClient, MediaClip clipData, string baseUrl, string outputFile, bool isBase64Init)
        {
            using (var fileStream = File.Create(outputFile))
            {
                if (isBase64Init)
                {
                    Console.WriteLine($"Writing initialize segment into {outputFile}");
                    var initSegment = Convert.FromBase64String(clipData.InitSegment);
                    fileStream.Write(initSegment, 0, initSegment.Length);
                }
                else
                {
                    var url = WebUtility.CombimeUrl(baseUrl, clipData.BaseUrl, clipData.InitSegment);
                    Console.WriteLine($"Downloading {url}");
                    await WebUtility.DownloadContentIntoStream(httpClient, url, fileStream);
                }

                foreach (var segment in clipData.Segments)
                {
                    var url = WebUtility.CombimeUrl(baseUrl, clipData.BaseUrl, segment.Url);
                    Console.WriteLine($"Downloading {url}");
                    await WebUtility.DownloadContentIntoStream(httpClient, url, fileStream);
                }
            }
        }
    }
}