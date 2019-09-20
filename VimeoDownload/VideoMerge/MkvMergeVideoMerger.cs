namespace VimeoDownload.VideoMerge
{
    public class MkvMergeVideoMerger : VideoMerger
    {
        /// <inheritdoc />
        protected override string ArgumentTemplate { get; } = "-o {2} {0} {1}";

        /// <inheritdoc />
        protected override string CommandLine { get; } = "mkvmerge";
    }
}