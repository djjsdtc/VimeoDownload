namespace VimeoDownload.VideoMerge
{
    /// <summary>
    /// 调用 ffmpeg 进行音视频合并的实现。
    /// 需要系统环境变量中包含可执行文件“ffmpeg”。
    /// </summary>
    public class FFmpegVideoMerger : VideoMerger
    {
        /// <inheritdoc />
        protected override string ArgumentTemplate { get; } = "-i {0} -i {1} -c copy -y {2}";

        /// <inheritdoc />
        protected override string CommandLine { get; } = "ffmpeg";
    }
}