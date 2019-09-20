namespace VimeoDownload.Wrapper.Windows
{
    using System;

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(string.Join(" ", args));
            ProgramEntrance.Main(args);
        }
    }
}
