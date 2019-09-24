namespace VimeoDownload.VideoMerge
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;

    /// <summary>
    /// 调用外部程序执行音视频合并操作的实现类。
    /// </summary>
    public abstract class VideoMerger
    {
        /// <summary>
        /// 调用的命令行参数模板。模板中 {0} 为视频文件输入，{1} 为音频文件输入，{2} 为输出文件。
        /// </summary>
        protected abstract string ArgumentTemplate { get; }

        /// <summary>
        /// 调用的外部程序命令行。
        /// </summary>
        protected abstract string CommandLine { get; }

        /// <summary>
        /// 外部程序输出处理方法。输入为外部函数输出的字符串。
        /// </summary>
        public Action<string> HandleMergerOutput { get; set; }

        /// <summary>
        /// 调用外部程序执行文件合并。
        /// </summary>
        /// <param name="videoFile">视频文件名。</param>
        /// <param name="audioFile">音频文件名。</param>
        /// <param name="output">输出文件名。</param>
        /// <returns>所执行的外部程序的返回值。</returns>
        /// <exception cref="Exception">如果外部程序不存在则抛出异常。</exception>
        public int MergeVideo(string videoFile, string audioFile, string output)
        {
            var argument = string.Format(ArgumentTemplate, $"\"{videoFile}\"", $"\"{audioFile}\"", $"\"{output}\"");
            Console.WriteLine($"Calling {CommandLine} with arguments: {argument}");
            var processInfo = new ProcessStartInfo
            {
                FileName = this.CommandLine,
                Arguments = argument,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            using (var process = new Process { StartInfo = processInfo })
            {
                var stdoutHandler = new DataReceivedEventHandler((sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        HandleMergerOutput(e.Data);
                    }
                });
                process.OutputDataReceived += stdoutHandler;
                process.ErrorDataReceived += stdoutHandler;

                try
                {
                    process.Start();
                }
                catch (Win32Exception)
                {
                    throw new Exception($"Could not launch {CommandLine}, make sure this program is in your PATH environment variable.");
                }

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
                return process.ExitCode;
            }
        }
    }
}