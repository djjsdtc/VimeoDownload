namespace VimeoDownload
{
    using CommandLine;

    /// <summary>
    /// 命令行选项。
    /// </summary>
    public partial class CommandLineOption
    {
        /// <summary>
        /// master.json 的文件地址。
        /// </summary>
        [Value(0, 
            HelpText = "The master.json metafile URL of the video to download.", 
            MetaName = "URL", Required = true)]
        public string DownloadAddress { get; set; }

        /// <summary>
        /// 如果为 <see langword="true" />，则进入下载模式。此时会忽略 <see cref="ListFormats" />。
        /// </summary>
        [Option('d', "download", 
            HelpText = "Download the Vimeo video.\n" +
                       "If this option is defined, '--list-formats' will be ignored.",
            Required = false)]
        public bool Download { get; set; }

        /// <summary>
        /// 如果为 <see langword="true" />，则下载完后不会调用外部工具进行合并。
        /// 此时会忽略 <see cref="OutputFileName" /> 和 <see cref="MergerName" />。
        /// </summary>
        [Option("no-merge",
            HelpText = "Keep temporary video and audio segments, do not merge into one media file.\n" +
                       "If this option is defined, '--output' and '--merger' will be ignored.",
            Required = false)]
        public bool NoMerge { get; set; }

        /// <summary>
        /// 指定外部合并程序的名称，目前支持“ffmpeg”和“mkvmerge”，默认为“ffmpeg”。
        /// </summary>
        [Option("merger",
            HelpText = "Define the video merger name. Supported values are 'ffmpeg' and 'mkvmerge'.\n" +
                       "If not defined or given a wrong value, ffmpeg will be used as the merger.",
            Default = "ffmpeg", MetaValue = "mergerName")]
        public string MergerName { get; set; } = "ffmpeg";

        /// <summary>
        /// 如果为 <see langword="true" />，则进入显示音视频格式列表模式。
        /// </summary>
        [Option('l', "list-formats",
            HelpText = "List all video and audio formats available for download.",
            Required = false)]
        public bool ListFormats { get; set; }

        /// <summary>
        /// 输出文件名。默认为“output.mp4”。
        /// </summary>
        [Option('o', "output",
            HelpText = "Define the output file name.", Default = "output.mp4",
            MetaValue = "outputFile")]
        public string OutputFileName { get; set; }

        /// <summary>
        /// 如果为 <see langword="true" />，则输出文件已存在时将覆盖。此时会忽略 <see cref="NotOverrideOutput" />。
        /// </summary>
        [Option("override",
            HelpText = "Override the output and temporary file if exists.\n" +
                       "If this option is defined, '--no-override' will be ignored.",
            Required = false)]
        public bool OverrideOutput { get; set; }

        /// <summary>
        /// 如果为 <see langword="true" />，则输出文件已存在时将跳过。
        /// 如果 <see cref="OverrideOutput" /> 和 <see cref="NotOverrideOutput" /> 均为 <see langword="false" />，则会在控制台询问用户是否覆盖文件。
        /// </summary>
        [Option("no-override",
            HelpText = "Do not override the output and temporary file if exists.\n" +
                       "If neither '--override' nor '--no-override' is defined and the output file exists, the program will ask if user would override the file or not.",
            Required = false)]
        public bool NotOverrideOutput { get; set; }

        /// <summary>
        /// 视频格式 ID，默认为最高画质。视频格式 ID 可在格式列表中获取。
        /// </summary>
        [Option("video",
            HelpText = "Define the video format id. The format id can be found from format list.\n" +
                       "If not defined or given a wrong value, the highest quality format will be downloaded.",
            Required = false, MetaValue = "videoId")]
        public string VideoFormatId { get; set; }

        /// <summary>
        /// 音频格式 ID，默认为最高音质。音频格式 ID 可在格式列表中获取。
        /// </summary>
        [Option("audio",
            HelpText = "Define the audio format id. The format id can be found from format list.\n" +
                       "If not defined or given a wrong value, the highest quality format will be downloaded.",
            Required = false, MetaValue = "audioId")]
        public string AudioFormatId { get; set; }

        /// <summary>
        /// 同时下载的线程数，默认为 4。
        /// </summary>
        [Option('t', "threads",
            HelpText = "Define the download thread's number.", Default = 4,
            MetaValue = "threadNum")]
        public int ThreadNumber { get; set; } = 4;

        /// <summary>
        /// 代理服务器地址。格式为 URL 格式（如“socks://127.0.0.1:1080”）。
        /// 也可指定为“none”（不使用代理）或“system”（使用环境变量或系统设置的代理）。
        /// 默认为“system”。
        /// </summary>
        [Option("proxy",
            HelpText =
                "Define the proxy to be used for download. You can use 'none', 'system' or url formatted proxy definition.\n" +
                "Currently supported proxy types are 'http' and 'socks'(SOCKS v5).\n" +
                "If not defined or use 'system', the environment variable or system setting will be applied.\n" +
                "If 'none' is defined, the system proxy will be bypassed.",
            Default = "system", MetaValue = "proxy")]
        public string Proxy { get; set; } = "system";

        /// <summary>
        /// HTTP 请求超时时间。单位为秒，默认值为 60 秒。
        /// </summary>
        [Option("timeout", HelpText = "Define the HTTP request timeout in seconds.", Default = 60)]
        public int Timeout { get; set; } = 60;

        /// <summary>
        /// 最大重试次数，默认为 3。
        /// </summary>
        [Option("retry", HelpText = "Define the maximum retry time when segment download fails.", Default = 3)]
        public int MaxRetry { get; set; } = 3;

        /// <summary>
        /// （仅GUI使用）生成文件的输出目录。
        /// </summary>
        public string OutputPath { get; set; }
    }
}