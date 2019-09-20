namespace VimeoDownload
{
    using CommandLine;

    public class CommandLineOption
    {
        [Value(0, 
            HelpText = "The master.json metafile URL of the video to download.", 
            MetaName = "URL", Required = true)]
        public string DownloadAddress { get; set; }

        [Option('d', "download", 
            HelpText = "Download the Vimeo video.", Required = false)]
        public bool Download { get; set; }

        [Option('l', "list-formats",
            HelpText = "List all video and audio formats available for download.",
            Required = false)]
        public bool ListFormats { get; set; }

        [Option('o', "output",
            HelpText = "Define the output file name.", Default = "output.mp4")]
        public string OutputFileName { get; set; }

        [Option("video",
            HelpText = "Define the video format id. The format id can be found from format list.\n" +
                       "If not defined, the highest quality format will be downloaded.")]
        public string VideoFormatId { get; set; }

        [Option("audio",
            HelpText = "Define the audio format id. The format id can be found from format list.\n" +
                       "If not defined, the highest quality format will be downloaded.")]
        public string AudioFormatId { get; set; }
    }
}