namespace VimeoDownload
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;

    public class VimeoDownloader
    {
        public string DownloadAddress { get; set; }

        public string OutputFilename { get; set; }

        public async Task DownloadVideo()
        {
            using (var httpClient = new HttpClient())
            {
                var videoInfo = await WebUtility.GetVideoInfo(httpClient, this.DownloadAddress);
                Console.WriteLine(videoInfo);
            }
        }
    }
}