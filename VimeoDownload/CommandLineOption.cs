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
            HelpText = "Download the Vimeo video.\n" +
                       "If this option is defined, '--list-formats' will be ignored.",
            Required = false)]
        public bool Download { get; set; }

        [Option("no-merge",
            HelpText = "Keep temporary video and audio segments, do not merge into one media file.\n" +
                       "If this option is defined, '--output' and '--merger' will be ignored.",
            Required = false)]
        public bool NoMerge { get; set; }

        [Option("merger",
            HelpText = "Define the video merger name. Supported values are 'ffmpeg' and 'mkvmerge'.\n" +
                       "If not defined or given a wrong value, ffmpeg will be used as the merger.",
            Default = "ffmpeg")]
        public string MergerName { get; set; }

        [Option('l', "list-formats",
            HelpText = "List all video and audio formats available for download.",
            Required = false)]
        public bool ListFormats { get; set; }

        [Option('o', "output",
            HelpText = "Define the output file name.", Default = "output.mp4")]
        public string OutputFileName { get; set; }

        [Option("override",
            HelpText = "Override the output file if exists.\n" +
                       "If this option is defined, '--no-override' will be ignored.",
            Required = false)]
        public bool OverrideOutput { get; set; }

        [Option("no-override",
            HelpText = "Do not override the output file if exists.\n" +
                       "If neither '--override' nor '--no-override' is defined and the output file exists, the program will ask if user would override the file or not.",
            Required = false)]
        public bool NotOverrideOutput { get; set; }

        [Option("video",
            HelpText = "Define the video format id. The format id can be found from format list.\n" +
                       "If not defined or given a wrong value, the highest quality format will be downloaded.",
            Required = false)]
        public string VideoFormatId { get; set; }

        [Option("audio",
            HelpText = "Define the audio format id. The format id can be found from format list.\n" +
                       "If not defined or given a wrong value, the highest quality format will be downloaded.",
            Required = false)]
        public string AudioFormatId { get; set; }
    }
}