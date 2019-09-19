namespace VimeoDownload
{
    using CommandLine;

    public class CommandLineOption
    {
        [Option('d', "download", HelpText = "Download video from given Vimeo metafile.", Required = true)]
        public string DownloadAddress { get; set; }

        [Option('o', "output", HelpText = "Define the output file name.", Default = "output.mp4")]
        public string OutputFileName { get; set; }
    }
}