namespace VimeoDownload
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using VimeoDownload.DataContract;
    using VimeoDownload.VideoMerge;

    public class VimeoDownloader : IDisposable
    {
        public string DownloadAddress { get; set; }

        public string OutputFilename { get; set; }

        public bool OverrideOutput { get; set; }

        public bool NotOverrideOutput { get; set; }

        public VideoMerger VideoMerger { get; set; }

        public string VideoFormatId { get; set; }

        public string AudioFormatId { get; set; }

        public int ThreadNumber { get; set; }

        private HttpClient httpClient = new HttpClient();

        public async Task ShowMediaInfo()
        {
            Console.WriteLine("Downloading metadata...");
            var videoInfo = await WebUtility.GetVideoInfo(httpClient, this.DownloadAddress);
            Console.WriteLine("Available video formats:");
            Console.WriteLine("clip id\t\tcodec\tresolution\tframerate");
            foreach (var video in videoInfo.Video)
            {
                Console.WriteLine($"{video.Id}\t{video.Codecs.Split('.')[0]}\t{video.Width}x{video.Height}\t\t{video.Framerate:0.###}fps");
            }

            Console.WriteLine();
            Console.WriteLine("Available audio formats:");
            Console.WriteLine("clip id\t\tcodec\tbitrate");
            foreach (var audio in videoInfo.Audio)
            {
                Console.WriteLine($"{audio.Id}\t{audio.Codecs.Split('.')[0]}\t{audio.Bitrate}");
            }
        }

        public async Task DownloadVideo()
        {
            if (VideoMerger != null && !ShouldCreateFile(OutputFilename))
            {
                return;
            }
            Console.WriteLine("Downloading metadata...");
            var videoInfo = await WebUtility.GetVideoInfo(httpClient, this.DownloadAddress);

            var tempDir = Directory.CreateDirectory(videoInfo.ClipId);
            Console.WriteLine($"Created directory {tempDir.FullName} to store temporary segments.");

            var baseUrl = WebUtility.CombimeUrl(this.DownloadAddress, videoInfo.BaseUrl);

            bool isVideoFileCreated = false;
            var videoFile = Path.Combine(tempDir.FullName, $"{videoInfo.ClipId}.m4v");
            if (ShouldCreateFile(videoFile))
            {
                isVideoFileCreated = true;
                var video = videoInfo.Video.FirstOrDefault(x => string.Equals(x.Id, this.VideoFormatId));
                if (video == null)
                {
                    Console.WriteLine("Video format not defined or wrong, use best quality instead.");
                    video = videoInfo.Video.First();
                }

                await DownloadMediaClip(httpClient, video, baseUrl, videoFile, videoInfo.IsBase64Init);
            }

            bool isAudioFileCreated = false;
            var audioFile = Path.Combine(tempDir.FullName, $"{videoInfo.ClipId}.m4a");
            if (ShouldCreateFile(audioFile))
            {
                isAudioFileCreated = true;
                var audio = videoInfo.Audio.FirstOrDefault(x => string.Equals(x.Id, this.AudioFormatId));
                if (audio == null)
                {
                    Console.WriteLine("Audio format not defined or wrong, use best quality instead.");
                    audio = videoInfo.Audio.First();
                }

                await DownloadMediaClip(httpClient, audio, baseUrl, audioFile, videoInfo.IsBase64Init);
            }

            if (VideoMerger == null)
            {
                Console.WriteLine($"All segments downloaded. Please check folder {tempDir.FullName}");
            }
            else
            {
                var mergeResult = VideoMerger.MergeVideo(videoFile, audioFile, this.OutputFilename);
                if (mergeResult == 0)
                {
                    if (isVideoFileCreated)
                    {
                        TryDelete(videoFile);
                    }

                    if (isAudioFileCreated)
                    {
                        TryDelete(audioFile);
                    }

                    TryDelete(tempDir);
                }
                else
                {
                    throw new Exception($"Error occurred when merging files, the exit code is {mergeResult}");
                }
            }
        }

        private async Task DownloadMediaClip(HttpClient httpClient, MediaClip clipData, string baseUrl, string outputFile, bool isBase64Init)
        {
            using (var fileStream = File.Create(outputFile))
            {
                if (isBase64Init)
                {
                    Console.WriteLine($"Writing initialize segment into {Path.GetFileName(outputFile)}");
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
                    new ParallelOptions { MaxDegreeOfParallelism = this.ThreadNumber },
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

                    TryDelete(tempFileName);
                }

                TryDelete(tempDirectory);
            }
        }

        private bool ShouldCreateFile(string fileName)
        {
            if (this.OverrideOutput)
            {
                Console.WriteLine("The file will be overwritten.");
                return true;
            }

            if (File.Exists(fileName))
            {
                Console.Write($"File {Path.GetFileName(fileName)} already exists. ");
                if (this.NotOverrideOutput)
                {
                    Console.WriteLine("The file will not be overwritten.");
                    return false;
                }

                Console.Write($"Override? (y/n): ");
                while (true)
                {
                    var result = Console.ReadKey().KeyChar;
                    Console.WriteLine();
                    if (result == 'Y' || result == 'y')
                    {
                        return true;
                    }
                    else if (result == 'N' || result == 'n')
                    {
                        return false;
                    }

                    Console.Write("Invalid response. Please enter 'y' or 'n': ");
                }
            }

            return true;
        }

        private void TryDelete(string fileName)
        {
            try
            {
                File.Delete(fileName);
            }
            catch
            {
                Console.WriteLine($"Cannot delete temporary storage {Path.GetFileName(fileName)}, you can manually remove it.");
            }
        }

        private void TryDelete(DirectoryInfo directory)
        {
            try
            {
                directory.Delete();
            }
            catch
            {
                Console.WriteLine($"Cannot delete temporary storage {directory.FullName}, you can manually remove it.");
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            httpClient?.Dispose();
        }
    }
}