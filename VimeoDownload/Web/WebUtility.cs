namespace VimeoDownload.Web
{
    using Flurl;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using VimeoDownload.DataContract;

    /// <summary>
    /// Web 操作相关的工具类。
    /// </summary>
    public static class WebUtility
    {
        /// <summary>
        /// 获取 Vimeo 视频的元数据。
        /// </summary>
        /// <param name="httpClient">HTTP 客户端。</param>
        /// <param name="url">master.json 的文件地址。</param>
        /// <param name="maxRetry">重试次数。</param>
        /// <returns>Vimeo 视频元数据。</returns>
        /// <exception cref="Exception">HTTP 请求出错时会抛出异常。</exception>
        public static async Task<VimeoVideo> GetVideoInfo(HttpClient httpClient, string url, int maxRetry)
        {
            for (var i = 0; i < maxRetry + 1; i++)
            {
                try
                {
                    var response = await httpClient.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<VimeoVideo>(json);
                        result.Audio = result.Audio?.OrderByDescending(x => x.Bitrate).ToList() ?? new List<AudioClip>();
                        result.Video = result.Video?.OrderByDescending(x => x.Height).ThenByDescending(x => x.Framerate).ToList() ?? new List<VideoClip>();
                        result.IsBase64Init = IsBase64InitSegment(url);
                        return result;
                    }
                    else
                    {
                        Console.WriteLine($"Request {url} gets error code {(int)response.StatusCode} ({response.StatusCode.ToString()})");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error occurred in download: {(e is AggregateException ? e.InnerException : e)}");
                }
            }

            throw new Exception("Max retry time exceeded when downloading metadata.");
        }

        /// <summary>
        /// 将数据下载到给定的文件流中。
        /// </summary>
        /// <param name="httpClient">HTTP 客户端。</param>
        /// <param name="url">文件地址。</param>
        /// <param name="fileStream">输出文件流。</param>
        /// <param name="maxRetry">重试次数。</param>
        /// <exception cref="Exception">HTTP 请求出错时会抛出异常。</exception>
        public static async Task DownloadContentIntoStream(HttpClient httpClient, string url, FileStream fileStream, int maxRetry)
        {
            for (var i = 0; i < maxRetry + 1; i++)
            {
                try
                {
                    var response = await httpClient.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        await response.Content.CopyToAsync(fileStream);
                        return;
                    }
                    else
                    {
                        Console.WriteLine($"Request {url} gets error code {(int)response.StatusCode} ({response.StatusCode.ToString()})");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error occurred in download: {(e is AggregateException ? e.InnerException : e)}");
                }
            }

            throw new Exception("Max retry time exceeded when downloading content.");
        }

        /// <summary>
        /// 根据查询字符串判断起始分段是否为 Base64 编码。
        /// </summary>
        /// <param name="url">master.json 的文件地址。</param>
        /// <returns>如果起始分段是 Base64 编码，则返回 <see langword="true" />。</returns>
        public static bool IsBase64InitSegment(string url)
        {
            var base64Init = int.TryParse(new Url(url).QueryParams["base64_init"]?.ToString(), out var i) ? i : 0;
            return base64Init == 1;
        }

        /// <summary>
        /// 组合多段 URL，支持相对路径。
        /// </summary>
        /// <param name="baseUrl">根 URL。</param>
        /// <param name="relativeUrls">其他 URL 部分，支持相对路径。</param>
        /// <returns>组合出的完整 URL。</returns>
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