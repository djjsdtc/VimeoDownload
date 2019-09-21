namespace VimeoDownload
{
    using CommandLine;
    using CommandLine.Text;
    using System.Collections.Generic;

    public partial class CommandLineOption
    {
        /// <summary>
        /// 在“Usage”中出现的示例下载地址。
        /// </summary>
        private const string ExampleDownloadAddress = "https://skyfire.vimeocdn.com/test1/342688958/sep/video/test2/master.json?base64_init=1";

        /// <summary>
        /// “Usage”示例。
        /// </summary>
#if NET461
        [Usage(ApplicationAlias = "VimeoDownload.exe")]
#elif NETCOREAPP2_2
        [Usage(ApplicationAlias = "dotnet VimeoDownload.dll")]
#endif
        public static IEnumerable<Example> Examples
        {
            get
            {
                yield return new Example("Simply download the video to \"output.mp4\" with best video and audio quality", new UnParserSettings(),
                    new CommandLineOption { Download = true, DownloadAddress = ExampleDownloadAddress });
                yield return new Example("List all video and audio qualities", new UnParserSettings(),
                    new CommandLineOption { ListFormats = true, DownloadAddress = ExampleDownloadAddress });
                yield return new Example("Define other output file, video & audio quality and merge tool", UnParserSettings.WithGroupSwitchesOnly(),
                    new CommandLineOption { Download = true, DownloadAddress = ExampleDownloadAddress,
                        AudioFormatId = "1370478512", VideoFormatId = "1370478523", MergerName = "mkvmerge" });
                yield return new Example("Use proxy, set max thread number, and do not perform merge operation", UnParserSettings.WithGroupSwitchesOnly(),
                    new CommandLineOption { Download = true, DownloadAddress = ExampleDownloadAddress, NoMerge = true,
                        Proxy = "socks://127.0.0.1:1080", ThreadNumber = 5});
            }
        }
    }
}
