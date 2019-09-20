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
                var tempDir = Directory.CreateDirectory(videoInfo.ClipId);
                Console.WriteLine($"Created directory {tempDir.FullName} to store temporary segments.");
                var baseUrl = WebUtility.CombimeUrl(this.DownloadAddress, videoInfo.BaseUrl);
                await DownloadMediaClip(httpClient, videoInfo.Video.First(), baseUrl, Path.Combine(tempDir.FullName, $"{videoInfo.ClipId}.m4v"), videoInfo.IsBase64Init);
                await DownloadMediaClip(httpClient, videoInfo.Audio.First(), baseUrl, Path.Combine(tempDir.FullName, $"{videoInfo.ClipId}.m4a"), videoInfo.IsBase64Init);
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

                var tempPath = Path.Combine(Path.GetTempPath(), $"{clipData.Id}.{clipData.Codecs}");
                var tempDirectory = Directory.CreateDirectory(tempPath);

                Parallel.ForEach(clipData.Segments,
                    new ParallelOptions{MaxDegreeOfParallelism = 4},
                    segment =>
                {
                    using (var tempFile = File.Create(Path.Combine(tempDirectory.FullName, segment.Url)))
                    {
                        var url = WebUtility.CombimeUrl(baseUrl, clipData.BaseUrl, segment.Url);
                        Console.WriteLine($"Downloading {url}");
                        WebUtility.DownloadContentIntoStream(httpClient, url, tempFile).Wait();
                    }
                });

                Console.WriteLine($"Combining all segments into {outputFile}");
                foreach (var segment in clipData.Segments)
                {
                    var tempFileName = Path.Combine(tempDirectory.FullName, segment.Url);
                    using (var tempFile = File.OpenRead(tempFileName))
                    {
                        tempFile.CopyTo(fileStream);
                    }

                    File.Delete(tempFileName);
                }

                tempDirectory.Delete();
            }
        }
    }
}