namespace VimeoDownload.Wrapper.Core
{
    /// <summary>
    /// .NET Core 封装程序。
    /// </summary>
    class Program
    {
        /// <summary>
        /// 程序入口。
        /// </summary>
        /// <param name="args">命令行参数。</param>
        static void Main(string[] args)
        {
            ProgramEntrance.Main<CoreCommandLineOption>(args);
        }
    }
}
