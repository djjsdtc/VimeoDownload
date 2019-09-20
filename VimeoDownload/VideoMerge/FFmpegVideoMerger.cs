namespace VimeoDownload.VideoMerge
{
    public class FFmpegVideoMerger : VideoMerger
    {
        /// <inheritdoc />
        protected override string ArgumentTemplate { get; } = "-i {0} -i {1} -c copy -y {2}";

        /// <inheritdoc />
        protected override string CommandLine { get; } = "ffmpeg";
    }
}