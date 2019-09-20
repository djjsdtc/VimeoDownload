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

    public static class ProgramEntrance
    {
        public static void Main(string[] args)
        {
            Parser.Default.ParseArguments<CommandLineOption>(args)
                .WithParsed(option => Run(option).Wait());
        }

        public static async Task Run(CommandLineOption option)
        {
            if (option.ThreadNumber < 1)
            {
                throw new Exception("Invalid thread number.");
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
