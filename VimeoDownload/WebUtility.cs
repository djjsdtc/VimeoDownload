namespace VimeoDownload
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Flurl;
    using Newtonsoft.Json;
    using VimeoDownload.DataContract;

    public static class WebUtility
    {
        public static async Task<VimeoVideo> GetVideoInfo(HttpClient httpClient, string url)
        {
            var response = await httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Request {url} gets error code {(int)response.StatusCode} ({response.StatusCode.ToString()})");
            }

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<VimeoVideo>(json);
            result.Audio = result.Audio.OrderByDescending(x => x.Bitrate).ToList();
            result.Video = result.Video.OrderByDescending(x => x.Height).ToList();
            result.IsBase64Init = IsBase64InitSegment(url);
            return result;
        }

        public static async Task DownloadContentIntoStream(HttpClient httpClient, string url, FileStream fileStream)
        {
            var response = await httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Request {url} gets error code {response.StatusCode} ({response.StatusCode.ToString()})");
            }

            await response.Content.CopyToAsync(fileStream);
        }

        public static bool IsBase64InitSegment(string url)
        {
            var base64Init = int.TryParse(new Url(url).QueryParams["base64_init"]?.ToString(), out var i) ? i : 0;
            return base64Init == 1;
        }

        public static string CombimeUrl(string baseUrl, params string[] relativeUrls)
        {
            var uri = new Uri(baseUrl);
            foreach (var relativeUrl in relativeUrls)
            {
                uri = new Uri(uri, relativeUrl);
            }
            return uri.ToString();
        }
    }
}