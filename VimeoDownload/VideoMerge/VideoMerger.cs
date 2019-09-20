namespace VimeoDownload.VideoMerge
{
    using System;
    using System.Diagnostics;

    public abstract class VideoMerger
    {
        protected abstract string ArgumentTemplate { get; }

        protected abstract string CommandLine { get; }

        public int MergeVideo(string videoFile, string audioFile, string output)
        {
            var argument = string.Format(ArgumentTemplate, $"\"{videoFile}\"", $"\"{audioFile}\"", $"\"{output}\"");
            using (var process = Process.Start(CommandLine, argument))
            {
                if (process == null)
                {
                    throw new Exception($"Could not launch {CommandLine}, make sure this program is in your PATH environment variable.");
                }
                process.WaitForExit();
                return process.ExitCode;
            }
        }
    }
}