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
        /// 程序总入口。请注意虽然方法名为 Main，但本项目为 .NET Standard 项目，不能直接调用。
        /// 需要从 .NET Core 或 .NET Framework 封装程序处调用该方法。
        /// </summary>
        /// <typeparam name="T"><see cref="CommandLineOption" /> 的各平台重载。</typeparam>
        /// <param name="args">命令行参数。</param>
        public static void Main<T>(string[] args) where T : CommandLineOption
        {
            try
            {
                Parser.Default.ParseArguments<T>(args)
                    .WithParsed(option => Run(option).Wait());
            }
            catch (AggregateException e)
            {
                Console.WriteLine("Error: {0}", e.InnerException);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: {0}", e);
            }
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

            using (var vimeoDownloader = new VimeoDownloader(GetProxyHandler(option.Proxy)))
            {
                vimeoDownloader.DownloadAddress = option.DownloadAddress;
            
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
                    Console.WriteLine("Error: {0}", e.InnerException);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error: {0}", e);
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
