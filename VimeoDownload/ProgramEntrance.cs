namespace VimeoDownload
{
    using System;
    using System.Threading.Tasks;
    using CommandLine;

    public static class ProgramEntrance
    {
        public static void Main(string[] args)
        {
            Parser.Default.ParseArguments<CommandLineOption>(args)
                .WithParsed(option => Run(option).Wait());
        }

        public static async Task Run(CommandLineOption option)
        {
            var vimeoDownloader = new VimeoDownloader();
            vimeoDownloader.DownloadAddress = option.DownloadAddress;
            
            try
            {
                if (option.Download)
                {
                    await vimeoDownloader.DownloadVideo();
                }
                else if (option.ListFormats)
                {
                    await vimeoDownloader.ShowMediaInfo();
                }
            }
            catch (AggregateException e)
            {
                Console.WriteLine("Error: {0}", e.InnerException.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: {0}", e.Message);
            }
        }
    }
}
