namespace VimeoDownload.VideoMerge
{
    /// <summary>
    /// 调用 MkvToolNix 进行音视频合并的实现。
    /// 需要系统环境变量中包含可执行文件“mkvmerge”。
    /// </summary>
    public class MkvMergeVideoMerger : VideoMerger
    {
        /// <inheritdoc />
        protected override string ArgumentTemplate { get; } = "-o {2} {0} {1}";

        /// <inheritdoc />
        protected override string CommandLine { get; } = "mkvmerge";
    }
}