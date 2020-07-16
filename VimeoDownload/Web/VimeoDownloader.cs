namespace VimeoDownload.Web
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using VimeoDownload.DataContract;
    using VimeoDownload.VideoMerge;

    /// <summary>
    /// 下载的主要逻辑实现。
    /// </summary>
    public class VimeoDownloader : IDisposable
    {
        /// <summary>
        /// master.json 的文件地址。
        /// </summary>
        public string DownloadAddress { get; set; }

        /// <summary>
        /// 输出文件名。
        /// </summary>
        public string OutputFilename { get; set; }

        /// <summary>
        /// 如果为 <see langword="true" />，则输出文件已存在时将覆盖。此时会忽略 <see cref="NotOverrideOutput" />。
        /// </summary>
        public bool OverrideOutput { get; set; }

        /// <summary>
        /// 如果为 <see langword="true" />，则输出文件已存在时将跳过。
        /// 如果 <see cref="OverrideOutput" /> 和 <see cref="NotOverrideOutput" /> 均为 <see langword="false" />，则会在控制台询问用户是否覆盖文件。
        /// </summary>
        public bool NotOverrideOutput { get; set; }

        /// <summary>
        /// 外部视频合并程序。如果为 <see langword="null" />，则不合并音视频。
        /// </summary>
        public VideoMerger VideoMerger { get; set; }

        /// <summary>
        /// 视频格式 ID。如果为 <see langword="null" />，则取最高画质。
        /// </summary>
        public string VideoFormatId { get; set; }

        /// <summary>
        /// 音频格式 ID。如果为 <see langword="null" />，则取最高音质。
        /// </summary>
        public string AudioFormatId { get; set; }

        /// <summary>
        /// 同时下载的线程数。
        /// </summary>
        public int ThreadNumber { get; set; }

        /// <summary>
        /// 最大重试次数。
        /// </summary>
        public int MaxRetry { get; set; }

        /// <summary>
        /// HTTP 客户端。
        /// </summary>
        private readonly HttpClient httpClient;

        /// <summary>
        /// 交互式操作中询问用户是否覆盖已存在文件的方法。
        /// 方法参数为要覆盖的文件名，返回结果为 <see langword="true" /> 则表示覆盖。
        /// </summary>
        private readonly Func<string, bool> overridePromotion;

        /// <summary>
        /// 构造函数。
        /// </summary>
        /// <param name="httpClientHandler">带代理设置的 <see cref="HttpClientHandler" />。</param>
        /// <param name="timeout">HTTP 请求超时时间。</param>
        /// <param name="overridePromotion">交互式操作中询问用户是否覆盖已存在文件的方法。</param>
        public VimeoDownloader(HttpClientHandler httpClientHandler, int timeout, Func<string, bool> overridePromotion)
        {
            this.overridePromotion = overridePromotion;
            this.httpClient = new HttpClient(httpClientHandler)
            {
                Timeout = TimeSpan.FromSeconds(timeout)
            };
        }

        /// <summary>
        /// （仅命令行使用）显示音视频格式列表。
        /// </summary>
        public async Task ShowMediaInfo()
        {
            Console.WriteLine("Downloading metadata...");
            var videoInfo = await WebUtility.GetVideoInfo(httpClient, this.DownloadAddress, this.MaxRetry);
            Console.WriteLine($"Video length: {TimeSpan.FromSeconds(videoInfo.Video[0].Duration).ToString()}");

            Console.WriteLine("Available video formats:");
            Console.WriteLine("{0,-15}{1,-8}{2,-15}{3}", "clip id", "codec", "resolution", "framerate");
            foreach (var video in videoInfo.Video)
            {
                Console.WriteLine("{0,-15}{1,-8}{2,-15}{3}", video.Id, video.Codecs.Split('.')[0], $"{video.Width}x{video.Height}", $"{video.Framerate:0.###}fps");
            }

            Console.WriteLine();
            Console.WriteLine("Available audio formats:");
            Console.WriteLine("{0,-15}{1,-8}{2}", "clip id", "codec", "bitrate");
            foreach (var audio in videoInfo.Audio)
            {
                Console.WriteLine("{0,-15}{1,-8}{2}", audio.Id, audio.Codecs.Split('.')[0], $"{audio.Bitrate / 1000.0}kbps");
            }
        }

        /// <summary>
        /// 下载视频。
        /// </summary>
        public async Task DownloadVideo()
        {
            if (VideoMerger != null && !ShouldCreateFile(OutputFilename, this.overridePromotion))
            {
                return;
            }
            Console.WriteLine("Downloading metadata...");
            var videoInfo = await WebUtility.GetVideoInfo(httpClient, this.DownloadAddress, this.MaxRetry);

            var tempDir = Directory.CreateDirectory(videoInfo.ClipId);
            Console.WriteLine($"Created directory {tempDir.FullName} to store temporary segments.");

            var baseUrl = WebUtility.CombimeUrl(this.DownloadAddress, videoInfo.BaseUrl);

            bool isVideoFileCreated = false;
            var videoFile = Path.Combine(tempDir.FullName, $"{videoInfo.ClipId}.m4v");
            if (ShouldCreateFile(videoFile, this.overridePromotion))
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
            if (ShouldCreateFile(audioFile, this.overridePromotion))
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
                if (mergeResult == 0)   // 外部程序返回 0 表示正常退出。
                {
                    if (isVideoFileCreated)
                    {
                        // 如果 videoFile 不是本次运行创建的则不应删除。
                        TryDelete(videoFile);
                    }

                    if (isAudioFileCreated)
                    {
                        // 如果 audioFile 不是本次运行创建的则不应删除。
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

        /// <summary>
        /// 下载音频/视频文件。
        /// </summary>
        /// <param name="httpClient">HTTP 客户端。</param>
        /// <param name="clipData">文件元数据。</param>
        /// <param name="baseUrl">文件的根路径。</param>
        /// <param name="outputFile">输出文件名。</param>
        /// <param name="isBase64Init">指示音频和视频文件的起始分段表示为 Base64 还是文件路径。</param>
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
                    await WebUtility.DownloadContentIntoStream(httpClient, url, fileStream, this.MaxRetry);
                }

                if (clipData.Segments != null && clipData.Segments.Count > 0)
                {
                    // segments 存放在系统的临时文件夹中，且始终覆盖。
                    var tempPath = Path.Combine(Path.GetTempPath(), $"{clipData.Id}.{clipData.Codecs}");
                    var tempDirectory = Directory.CreateDirectory(tempPath);

                    Parallel.For(0, clipData.Segments.Count,
                        new ParallelOptions {MaxDegreeOfParallelism = this.ThreadNumber},
                        i =>
                        {
                            var segment = clipData.Segments[i];
                            using (var tempFile = File.Create(Path.Combine(tempDirectory.FullName, segment.Url)))
                            {
                                var url = WebUtility.CombimeUrl(baseUrl, clipData.BaseUrl, segment.Url);
                                Console.WriteLine($"Downloading segment {i + 1} of {clipData.Segments.Count}: {url}");
                                WebUtility.DownloadContentIntoStream(httpClient, url, tempFile, this.MaxRetry).Wait();
                            }
                        });

                    Console.WriteLine($"Combining all segments into {Path.GetFileName(outputFile)}");
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
        }

        /// <summary>
        /// 确定是否应该创建新文件。如果文件不存在或文件存在且允许覆盖，则返回 <see langword="true" />。
        /// </summary>
        /// <param name="fileName">文件名。</param>
        /// <param name="overridePromotion">交互式操作中询问用户是否覆盖已存在文件的方法。</param>
        /// <returns>如果允许创建新文件，则返回 <see langword="true" />。</returns>
        private bool ShouldCreateFile(string fileName, Func<string, bool> overridePromotion)
        {
            if (File.Exists(fileName))
            {
                Console.Write($"File {Path.GetFileName(fileName)} already exists. ");

                if (this.OverrideOutput)
                {
                    Console.WriteLine("The file will be overwritten.");
                    return true;
                }
                if (this.NotOverrideOutput)
                {
                    Console.WriteLine("The file will not be overwritten.");
                    return false;
                }

                return overridePromotion(fileName);
            }

            return true;
        }

        /// <summary>
        /// 删除文件，如删除失败则提示用户手动删除。
        /// </summary>
        /// <param name="fileName">文件名。</param>
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

        /// <summary>
        /// 删除目录，如删除失败则提示用户手动删除。
        /// </summary>
        /// <param name="directory">要删除的目录对象。</param>
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