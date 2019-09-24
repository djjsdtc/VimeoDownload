namespace VimeoDownload.GUI
{
    /// <summary>
    /// 保存设置用的常量
    /// </summary>
    public static class SettingsConst
    {
        /// <summary>
        /// 输出文件路径。
        /// </summary>
        public const string OutputPath = "OutputPath";

        /// <summary>
        /// 指示是否合并音视频文件。
        /// </summary>
        public const string MergeOutput = "MergeOutput";

        /// <summary>
        /// 合并器名称。
        /// </summary>
        public const string Merger = "Merger";

        /// <summary>
        /// 合并器名称为“ffmpeg”。
        /// </summary>
        public const string MergerFFMpeg = "ffmpeg";

        /// <summary>
        /// 合并器名称为“mkvmerge”。
        /// </summary>
        public const string MergerMkvMerge = "mkvmerge";

        /// <summary>
        /// 线程数。
        /// </summary>
        public const string ThreadNumber = "ThreadNumber";

        /// <summary>
        /// 代理服务器。
        /// </summary>
        public const string Proxy = "Proxy";

        /// <summary>
        /// 代理服务器类型为“none”。
        /// </summary>
        public const string ProxyTypeNone = "none";

        /// <summary>
        /// 代理服务器类型为“system”。
        /// </summary>
        public const string ProxyTypeSystem = "system";

        /// <summary>
        /// 代理服务器类型为“http”。
        /// </summary>
        public const string ProxyTypeHttp = "http";

        /// <summary>
        /// 代理服务器类型为“socks”。
        /// </summary>
        public const string ProxyTypeSocks = "socks";

        /// <summary>
        /// 超时时间。
        /// </summary>
        public const string Timeout = "Timeout";

        /// <summary>
        /// 重试次数。
        /// </summary>
        public const string RetryTime = "RetryTime";
    }
}