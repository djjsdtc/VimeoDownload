namespace VimeoDownload
{
    using System;
    using System.Threading.Tasks;
    using CommandLine;
    using VimeoDownload.VideoMerge;

    public static class ProgramEntrance
    {
        public static void Main(string[] args)
        {
            Parser.Default.ParseArguments<CommandLineOption>(args)
                .WithParsed(option => Run(option).Wait());
        }

        public static async Task Run(CommandLineOption option)
        {
            using (var vimeoDownloader = new VimeoDownloader())
            {
                vimeoDownloader.DownloadAddress = option.DownloadAddress;
            
                try
                {
                    if (option.Download)
                    {
                        vimeoDownloader.OutputFilename = option.OutputFileName;
                        vimeoDownloader.AudioFormatId = option.AudioFormatId;
                        vimeoDownloader.VideoFormatId = option.VideoFormatId;
                        if (!option.NoMerge)
                        {
                            vimeoDownloader.VideoMerger = GetVideoMerger(option.MergerName);
                        }

                        await vimeoDownloader.DownloadVideo();
                    }
                    else if (option.ListFormats)
                    {
                        await vimeoDownloader.ShowMediaInfo();
                    }
                }
                catch (AggregateException e)
                {
                    Console.WriteLine("Error: {0}", e.InnerException);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error: {0}", e);
                }
            }
        }

        public static VideoMerger GetVideoMerger(string mergerName)
        {
            switch (mergerName.ToLower())
            {
                case "mkvmerge":
                    return new MkvMergeVideoMerger();
                case "ffmpeg":
                default:
                    return new FFmpegVideoMerger();
            }
        }
    }
}
