namespace VimeoDownload
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using VimeoDownload.DataContract;

    public static class WebUtility
    {
        public static async Task<VimeoVideo> GetVideoInfo(string url)
        {
            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Request {url} gets error code {response.StatusCode} ({response.StatusCode.ToString()})");
                }

                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<VimeoVideo>(json);
            }
        }

        public static async Task DownloadContentIntoStream(string url, FileStream fileStream)
        {
            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Request {url} gets error code {response.StatusCode} ({response.StatusCode.ToString()})");
                }

                await response.Content.CopyToAsync(fileStream);
            }
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