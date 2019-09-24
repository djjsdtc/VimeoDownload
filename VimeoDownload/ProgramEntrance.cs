namespace VimeoDownload
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using CommandLine;
    using MihaZupan;
    using VimeoDownload.VideoMerge;
    using VimeoDownload.Web;

    /// <summary>
    /// 程序总入口。
    /// </summary>
    public static class ProgramEntrance
    {
        /// <summary>
        /// 程序总入口。
        /// </summary>
        /// <param name="args">命令行参数。</param>
        /// <returns>如果程序正确执行，返回 0。如果出错，返回 -1。</returns>
        public static int Main(string[] args)
        {
            try
            {
                Parser.Default.ParseArguments<CommandLineOption>(args)
                    .WithParsed(option => Run(option).Wait());
                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {(e is AggregateException ? e.InnerException : e)}");
            }

            return -1;
        }

        /// <summary>
        /// 执行程序。
        /// </summary>
        /// <param name="option">解析后的命令行参数。</param>
        public static async Task Run(CommandLineOption option)
        {
            if (option.ThreadNumber < 1)
            {
                throw new Exception("Invalid thread number.");
            }

            if (!option.Download && !option.ListFormats)
            {
                throw new Exception("One of '--download' and '--list-formats' must be defined.");
            }

            if(option.MaxRetry < 0)
            {
                throw new Exception("Invalid maximum retry time.");
            }

            if (option.Timeout < 1)
            {
                throw new Exception("Invalid timeout.");
            }

            using (var vimeoDownloader = new VimeoDownloader(GetProxyHandler(option.Proxy), option.Timeout))
            {
                vimeoDownloader.DownloadAddress = option.DownloadAddress;
                vimeoDownloader.MaxRetry = option.MaxRetry;
            
                try
                {
                    if (option.Download)
                    {
                        vimeoDownloader.OutputFilename = option.OutputFileName;
                        vimeoDownloader.AudioFormatId = option.AudioFormatId;
                        vimeoDownloader.VideoFormatId = option.VideoFormatId;
                        vimeoDownloader.OverrideOutput = option.OverrideOutput;
                        vimeoDownloader.NotOverrideOutput = option.NotOverrideOutput;
                        vimeoDownloader.ThreadNumber = option.ThreadNumber;

                        if (!option.NoMerge)
                        {
                            vimeoDownloader.VideoMerger = GetVideoMerger(option.MergerName);
                        }

                        await vimeoDownloader.DownloadVideo();
                    }
                    else if (option.ListFormats)
                    {
                        await vimeoDownloader.ShowMediaInfo();
                    }
                }
                catch (AggregateException e)
                {
                    throw e.InnerException;
                }
            }
        }

        /// <summary>
        /// 根据给定的合并器名称获取外部视频合并器的实现。
        /// </summary>
        /// <param name="mergerName">合并器名称。</param>
        /// <returns>合并器实现。如果名称不存在则返回 ffmpeg 合并器。</returns>
        public static VideoMerger GetVideoMerger(string mergerName)
        {
            switch (mergerName.ToLower())
            {
                case "mkvmerge":
                    return new MkvMergeVideoMerger();
                case "ffmpeg":
                    break;
                default:
                    Console.WriteLine($"Invalid video merger '{mergerName}', ffmpeg will be used.");
                    break;
            }

            return new FFmpegVideoMerger();
        }

        /// <summary>
        /// 根据给定的代理服务器设置返回对应的 <see cref="HttpClientHandler"/>。
        /// </summary>
        /// <param name="proxySetting">代理服务器设置。</param>
        /// <returns>对应的 <see cref="HttpClientHandler"/>。</returns>
        /// <exception cref="Exception">如果代理服务器设置有误或不支持则抛出异常。</exception>
        public static HttpClientHandler GetProxyHandler(string proxySetting)
        {
            if (proxySetting.ToLower() == "none")
            {
                return new HttpClientHandler {UseProxy = false};
            }
            else if (proxySetting.ToLower() == "system")
            {
                return new HttpClientHandler {UseProxy = true};
            }
            else
            {
                try
                {
                    var uri = new Uri(proxySetting);
                    if (uri.Scheme.ToLower() == "http")
                    {
                        return new HttpClientHandler
                        {
                            UseProxy = true,
                            Proxy = new WebProxy(uri.Host, uri.Port)
                        };
                    }
                    else if (uri.Scheme.ToLower() == "socks")
                    {
                        return new HttpClientHandler
                        {
                            UseProxy = true,
                            Proxy = new HttpToSocks5Proxy(uri.Host, uri.Port)
                        };
                    }
                    else
                    {
                        throw new UriFormatException();
                    }
                }
                catch (UriFormatException)
                {
                    throw new Exception($"Invalid proxy setting {proxySetting}");
                }
            }
        }
    }
}
